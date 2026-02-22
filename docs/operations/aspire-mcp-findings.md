# Aspire MCP findings (diagnostics)

This document summarizes issues found using the Aspire MCP tools (`list resources`, `list structured logs`, `list console logs`, `list traces`) per AGENTS.md.

**Fixes applied in code (this pass):**
- **move-fleet 404:** Client now sends `worldId` (from `CurrentSession.Id`) and `PlayerId` in the body; no session → returns false with warning log. Server route was already correct.
- **Session 500 – Invalid column name 'PlayerId':** Startup now runs **EnsurePlayerIdColumnAsync** after migrations: if the `Sessions` table exists but has no `PlayerId` column (e.g. app run with `--no-build` so the migration was never applied), it runs `ALTER TABLE [Sessions] ADD [PlayerId] nvarchar(200) NULL`. So the column is added on the next WebApi startup without requiring a full rebuild or manual `dotnet ef database update`.

## Resource status (list resources)

- **ravenDb**, **gameDb**, **redis**, **webapi**, **blazor**: all **Running**, **Healthy**.
- **sqlserver-password**, **redis-password**: Parameters, no health checks.

## Issues found and fixes

### 1. Session creation 500 – FK_Sessions_Clients_ClientId (fixed)

- **Source:** Console logs (webapi, blazor), structured log `log_id: 481` and webapi console.
- **Error:** `The INSERT statement conflicted with the FOREIGN KEY constraint "FK_Sessions_Clients_ClientId". The conflict occurred in database "StarConflictsRevolt", table "dbo.Clients", column 'Id'.`
- **Cause:** Player tracking sends a free-form client id (localStorage UUID or `anonymous-xxx`). That value was stored in `Session.ClientId`, which is a FK to `Clients.Id`. Those ids are not in `Clients`, so INSERT failed.
- **Fix:** Added `Session.PlayerId` (nullable string, no FK) for player-tracking id. Create session now sets `PlayerId = clientId`, `ClientId = null`. `GetActiveSessionsAsync` filters by `PlayerId`. Migration `AddPlayerIdToSession` adds the column. Run `dotnet ef database update` (or app startup migration) to apply.

### 2. SignalR JoinSessionAsync – CancellationTokenSource disposed (fixed)

- **Source:** Structured log **Error** `log_id: 481`: "Error joining session ... The CancellationTokenSource has been disposed."
- **Cause:** When SignalR wasn’t connected yet, `JoinSessionAsync` waited in a loop using `_cts.Token`. If the service was disposed (e.g. user navigated away), `_cts` was disposed and the next access to `_cts.Token` threw.
- **Fix:** Use a local `CancellationTokenSource` for the 15s wait (e.g. `new CancellationTokenSource(TimeSpan.FromSeconds(15))`) and use `CancellationToken.None` for the `SendAsync("JoinWorld", ...)` call so we don’t touch `_cts` during join.

### 3. ClientIdProvider – JS interop during prerender (fixed)

- **Source:** Blazor console: "Could not get client id from browser, using fallback" – `InvalidOperationException: JavaScript interop calls cannot be issued at this time. This is because the component is being statically rendered.`
- **Cause:** `GetClientIdAsync` was called during create-session flow that can run during prerender (e.g. Single Player start).
- **Fix:** Catch `InvalidOperationException` when message contains "statically rendered" and return fallback with `LogDebug` instead of `LogWarning`; keep fallback behavior.

### 4. POST /game/move-fleet returns 404 (fixed)

- **Source:** Blazor console: "POST https://localhost:7133/game/move-fleet" → 404.
- **Cause:** Server route exists but returns 404 when `worldId` query is missing or session not found (`GameActionEndpointHandler`: `if (worldId == Guid.Empty || !await sessionManagerService.SessionExistsAsync(worldId))`). Client was not sending `worldId` or `PlayerId` in the body.
- **Fix:** Client now sends `worldId` from `CurrentSession.Id` (required; returns false with log if no session) and `PlayerId` (from `ClientIdProvider`, parsed as Guid or `Guid.Empty`). `HttpApiClientExtensions.MoveFleetAsync` now takes `worldId` and `playerId`; body includes `PlayerId`, `FleetId`, `FromPlanetId`, `ToPlanetId`. Unit tests updated: `MoveFleetAsync_NoSession_ReturnsFalse` and `MoveFleetAsync_ValidParameters_WithSession_ReturnsTrue`.

### 5. EF Core “Duplicate of …” in structured logs (informational)

- **Source:** Structured logs from webapi: "Duplicate of \"message\" for log entry …", "Duplicate of attribute \"commandText\" …".
- **Cause:** OpenTelemetry/EF Core logging attribute deduplication.
- **Action:** No code change; diagnostic noise only.

## Structured logs exploration (latest)

A `list_structured_logs` run returned 47 recent entries (515 older truncated). Summary:

| Severity   | Resource | What appears |
|-----------|----------|--------------|
| **Error** | webapi   | DbCommand failing: `SELECT [s].[Id], [s].[ClientId], [s].[Created], [s].[Ended], [s].[IsActive], [s].[PlayerId], [s].[SessionName], [s].[SessionType] FROM [Sessions] AS [s] WHERE [s].[IsActive] = CAST(1 AS bit) AND [s].[PlayerId] = @playerId` — fails because **`PlayerId` column is missing** in DB (migration not applied). Followed by "Duplicate of …" (telemetry dedup) and "An unhandled exception has occurred while executing the request." |
| **Error** | webapi   | "Duplicate of \"message\" for log entry 454/457" and "Duplicate of attribute \"error\" for log entry 454" — deduplication; real error is in the referenced log entry (Invalid column name 'PlayerId'). |
| **Warning**| Blazor   | Polly: "Execution attempt … Result: '500'", "OnRetry" (Attempt 0, 1, 2). |
| **Warning**| Blazor   | `GameStateService`: "Server returned null response for session creation". |
| Info      | Blazor   | "Sending HTTP request POST https://localhost:7133/game/session", "Received HTTP response headers … 500". |

**Conclusion:** All current errors stem from **POST /game/session** returning 500 because the WebApi runs queries that reference `[s].[PlayerId]` while the `Sessions` table does not yet have that column. Apply the migration (see below) to resolve.

## How to re-check after changes (Aspire MCP)

**In Cursor**, the Aspire MCP server is exposed as **`user-aspire`** (not `aspire`). Use that when calling MCP tools.

1. Select app host: `select_apphost` with path to `StarConflictsRevolt.Aspire.AppHost.csproj` (if multiple app hosts).
2. **list_resources** (server `user-aspire`) – confirm all resources Running/Healthy (e.g. [ravenDb](https://localhost:17177/?resource=ravenDb-a56f944c), [webapi](https://localhost:17177/?resource=webapi-ncwuqudw), [blazor](https://localhost:17177/?resource=blazor-wnpyskcx), redis).
3. **list_structured_logs** – look for `severity: "Error"` or `"Warning"`; confirm no "Invalid column name 'PlayerId'" (obsolete after LiteDB pivot).
4. **list_console_logs** with `resource_name: "webapi"` – confirm no SQL 207 after migration; with `resource_name: "blazor"` – confirm no 404 on move-fleet when session exists.
5. **list_traces** with `has_error: true` – confirm POST /game/session and POST /game/move-fleet succeed (no 500/404).

**Latest Aspire MCP check (post–LiteDB):** `list_resources` showed ravenDb, webapi, redis, blazor, redis-password all **Running** and **Healthy**. No gameDb (removed). Recent `list_structured_logs` showed only Information-level Blazor auth messages; no Error/Warning in the returned window. Traces with `has_error: true` returned none with errors in the recent set.

### 6. Database 'StarConflictsRevolt' does not exist (fixed)

- **Source:** `DatabaseDoesNotExistException` when persisting events (e.g. CommandRejected) to RavenDB; ProcessLoop logs "Failed to persist event …" and "Error processing event in ProcessLoop".
- **Cause:** RavenDB server (e.g. Aspire container) was running but the named database had not been created.
- **Fix:** In **StartupHelper.RegisterRavenDb**, after initializing the DocumentStore we call **EnsureRavenDatabaseExists**: it uses `GetStatisticsOperation` to check if the database exists; if `DatabaseDoesNotExistException` is thrown, it sends **CreateDatabaseOperation** to create the database. So on first WebApi startup (or after a fresh RavenDB container), the database is created automatically.

## Session 500 – PlayerId column (fixed in startup)

**Cause:** EF model includes `Session.PlayerId` but the database had no such column (migration not applied, e.g. WebApi started with `--no-build`). Console showed "No pending migrations found" while queries still selected `[s].[PlayerId]` → SQL 207 Invalid column name.

**Fix in code:** In `StartupHelper.ConfigureAsync`, after running EF migrations we call **EnsurePlayerIdColumnAsync**: it checks `INFORMATION_SCHEMA.COLUMNS` for `Sessions.PlayerId` and, if missing, runs `ALTER TABLE [Sessions] ADD [PlayerId] nvarchar(200) NULL`. So on the **next WebApi restart** (no rebuild required), the column is added and session create/join/delete succeed.

**To pick up the fix:** Rebuild the WebApi project (so the new startup code is in the binary), then restart the Aspire app host. No need to run `dotnet ef database update` manually.
