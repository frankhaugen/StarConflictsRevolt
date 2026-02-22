# Playtest runbook (assistant-driven)

How to run a full playtest so the assistant (or you) can validate the game flow. Use this after code changes to confirm navigation, Single Player, and sessions work.

## Option A: Automated playtest (recommended for the assistant)

**No AppHost, no Docker, no Playwright MCP.** The assistant can run this to "play" the game:

```powershell
.\scripts\playtest.ps1
```

From repo root. This builds the test project, ensures Playwright browsers are installed, and runs all UI tests (NavigationTests, HomePageTests, etc.) against an in-process Blazor host with mocks. Exit code 0 = playable; non-zero = fix the failing tests.

**If build fails with "file is locked" (MSB3027):** Stop any running `StarConflictsRevolt.Aspire.AppHost`, Blazor, or WebApi processes, then run the script again.

## Option B: Manual playtest with Playwright MCP

If you use the Playwright MCP in Cursor for interactive playtesting against the full stack:

### Prerequisites

- Solution built: `dotnet build StarConflictsRevolt.slnx`
- Docker running (for Redis, SQL Server, RavenDB when using containers)
- Playwright MCP available in Cursor (e.g. **user-microsoft/playwright-mcp**)

## 1. Start the stack

From repo root:

```bash
dotnet run --project StarConflictsRevolt.Aspire.AppHost
```

Wait until the Aspire dashboard shows **webapi** and **blazor** as healthy. The Blazor app is typically at **https://localhost:7120** (see dashboard for the exact URL if ports differ).

### 2. Run playtest with Playwright MCP

Use the **Playwright MCP** (server: `user-microsoft/playwright-mcp`), not the cursor-ide-browser, so you get full accessibility snapshots and element refs for clicks.

1. **Navigate** to the Blazor app:
   - `browser_navigate` with `url: "https://localhost:7120"`

2. **Create a session (Single Player)**  
   - Click the Single Player button (use ref from `browser_snapshot`, e.g. the button with `data-testid="single-player-btn"` or the "Single Player" link to `/singleplayer`).  
   - Or navigate directly: `browser_navigate` to `https://localhost:7120/singleplayer`.  
   - Wait 2–5 seconds, then take a **snapshot**. You should see the game UI (Game Controls, Resources, and either "Loading galaxy..." or the galaxy map). If the session is created and SignalR is connected, the galaxy should load.

3. **Join an existing session**  
   - Navigate to `https://localhost:7120/sessions`.  
   - Take a snapshot; you should see the sessions table with Join buttons.  
   - Click **Join** on a session (use ref from snapshot).  
   - Navigate to `/singleplayer` or Galaxy View and verify the world/galaxy is shown.

4. **Sanity checks**  
   - Home page shows "Connecting to game server..." then "Connected" / "Not in a session" as appropriate.  
   - Sessions table no longer shows the "else { Full }" text in the Actions column (fixed in Sessions.razor).  
   - Single Player page shows either "Loading galaxy..." (briefly) or the galaxy map and resources.

### 3. What was fixed for playtesting

- **SignalR**: Connection starts at app startup (`SignalRStartupHostedService`). `JoinSessionAsync` waits up to 15s for the hub to be connected before joining the world group, so create/join no longer fail with "SignalR connection not established" when the connection is slightly delayed.
- **UI**: Home shows "Connecting to game server..." when SignalR is not yet connected; Debug shows SignalR and session state. Sessions table Actions column fixed (removed duplicate `else { Full }`).
- **Dev HTTPS**: Blazor `AllowInvalidHttpsForSignalR` in Development so the SignalR client can connect to the webapi over HTTPS with the dev certificate.

## See also

- [aspire.md](aspire.md) — AppHost, resources, HTTPS ports.
- [troubleshooting.md](troubleshooting.md) — Common issues.
- [../tooling/agents.md](../tooling/agents.md) — Use Playwright MCP for functional checks.
