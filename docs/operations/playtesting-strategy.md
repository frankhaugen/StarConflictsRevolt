# Playtesting strategy

A structured approach to testing StarConflictsRevolt by **playing** the game: validate the core loop, commands, and client–server behavior without relying only on automated tests.

---

## Goals

- **Confirm the loop works:** Create/join session → send commands → see updates (deltas/events) in the client.
- **Exercise all command entry points:** SignalR (GameHub) and, where available, REST.
- **Catch regressions:** After changes, re-run a short smoke + core scenario so obvious breakage is found quickly.
- **Inform work items:** Turn bugs or missing behavior into items in [../work-items/current.md](../work-items/current.md).

---

## Prerequisites

| Requirement | Check |
|-------------|--------|
| Solution builds | `dotnet build StarConflictsRevolt.slnx` succeeds. |
| Docker running | Redis, SQL Server, RavenDB containers can start. |
| AppHost running | `dotnet run --project StarConflictsRevolt.Aspire.AppHost`; dashboard URL in console. |
| Browser | Open Blazor from the Aspire dashboard (blazor resource). |

See [aspire.md](aspire.md) and [story.md](story.md) for run instructions.

---

## Test levels

| Level | When | What |
|-------|------|------|
| **Smoke** | Every playtest | App and UI load; can open sessions page and create or join. |
| **Core loop** | Every playtest | One full flow: create/join → one command (e.g. move fleet) → see update. |
| **Commands** | After command/hub changes | Each GameHub command (MoveFleet, QueueBuild, StartRally, StartMartialLaw) at least once. |
| **Multi-client** | When testing sync | Two browsers (or tabs) in same session; both see same world and updates. |
| **Regression** | After any change | Smoke + core loop before considering a change “done”. |

---

## Scenarios

### 1. Smoke

1. Open the Blazor app from the Aspire dashboard.
2. Navigate to the sessions/game entry point (e.g. Sessions or equivalent).
3. **Pass:** Page loads, no unhandled errors; you can see a list or “create session” option.

### 2. Core loop (create → command → update)

1. **Create session:** Create a new game session (name, type as offered by the UI).
2. **Join / enter world:** Join the session so the client is in the world (WorldHub JoinWorld).
3. **Send one command:** e.g. move a fleet to another system (use GameHub or UI that calls it).
4. **Observe:** Within a few ticks, the UI or world view should reflect the change (e.g. fleet location or order accepted).
5. **Pass:** Command is accepted and an update (delta/event) is visible in the client.

### 3. Commands (GameHub)

For each command the Blazor UI exposes (see [../reference/api-transport.md](../reference/api-transport.md)):

| Command | Action | What to check |
|---------|--------|----------------|
| MoveFleet | Move a fleet to another system | Fleet shows in transit or at destination; no client/server error. |
| QueueBuild | Queue a build at a system | Build appears in queue or system state updates. |
| StartRally | Start rally in a region | No error; relevant state/UI updates if implemented. |
| StartMartialLaw | Start martial law in a system | No error; system state/UI updates if implemented. |

Run at least one successful invocation per command; note any that fail or have no visible effect (might be stub or missing UI).

### 4. Multi-client (optional)

1. Open Blazor in two different browsers (or one normal + one incognito).
2. Join the **same** session with both.
3. From one client, send a command (e.g. move fleet).
4. **Pass:** The other client shows the same world update (e.g. fleet moved) without refresh.

### 5. REST command path (optional)

If the UI or a tool can trigger REST command endpoints (e.g. `POST .../commands/move-fleet`), run one move via REST and confirm the same world/update behavior as via GameHub.

---

## What to observe

- **UI:** No red errors, no infinite loading; buttons/actions respond; world/session view updates.
- **Network:** In browser DevTools, SignalR connection established; no 4xx/5xx on key requests.
- **Server:** In Aspire dashboard or logs, webapi/blazor healthy; no repeated exceptions.
- **Persistence (optional):** Restart AppHost, rejoin same session; world state is still consistent (if supported).

---

## Recording issues

- **Bug or missing behavior:** Add a work item to [../work-items/current.md](../work-items/current.md). Example: `[Blazor] Move fleet button does nothing when no fleet selected` with a one-line summary and, if useful, steps to reproduce.
- **Flaky test:** Note in the work item (e.g. “Intermittent: WorldHub drops connection after N minutes”).
- **Pass:** No need to log every pass; use the **Regression** level to guard against regressions.

---

## Suggested cadence

| When | Minimum |
|------|---------|
| Before merging a change that touches session/commands/hubs | Smoke + Core loop. |
| After adding or changing a GameHub command | Smoke + Core loop + that command. |
| Ad hoc / exploratory | Full Commands run; optionally Multi-client. |

Keep the smoke and core-loop steps short so they’re easy to run often.
