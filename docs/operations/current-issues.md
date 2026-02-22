# Current issues (for assistant to read)

## How the assistant can "play" the game

The assistant **can** play the game by running the **automated playtest** (no browser or Playwright MCP required):

```powershell
.\scripts\playtest.ps1
```

This runs Playwright UI tests against an in-process Blazor host (no AppHost needed). If the script exits 0, the game is playable; failures indicate what broke. Run from repo root. If build fails with "file is locked", stop any running AppHost/Blazor/WebApi processes first.

---

The assistant cannot open localhost in a human browser. To have it **see** runtime or UI issues:

1. **Paste** console output, error messages, or stack traces **below** (under "Pasted output").
2. **Screenshots**: save images in **docs/operations/current-issues/** (e.g. `error.png`, `blazor-screen.png`). The assistant can read image files from the repo.
3. **Describe** what you did and what you expected vs what happened.

After you update this file (and/or add screenshots), tell the assistant to look at **docs/operations/current-issues.md** and it will read the content and images to diagnose and fix.

---

## Pasted output

```
Paste Aspire dashboard logs, browser console errors, or terminal output here.
```

---

## What you did / what happened

(Optional: short description, e.g. "Clicked Join on session X, got SignalR not established" or "Blazor page shows blank after create session.")

---

## Resolved (2025-02-22)

- **No delete for galaxies/sessions**: Added delete session API and UI.
  - API: `DELETE /game/session/{sessionId}` (ends session in DB, removes in-memory aggregate).
  - Client: `DeleteSessionAsync(sessionId)` on `IHttpApiClient` and `IGameStateService`.
  - Sessions page: delete (trash) button per row; clears current session if you delete the one you’re in.
- **Galaxy not generating**: Fixed create-session flow so the galaxy is generated and returned.
  - Create session now builds the default world (with star systems and planets) via `WorldFactory.CreateDefaultWorld()`, assigns `world.Id = sessionId`, registers it with `SessionAggregateManager`, and returns that world in the response so the client gets the full galaxy.
  - Galaxy view no longer uses random positions: star systems are placed using `StarSystemDto.Coordinates` mapped to percentage (10–90%) so positions are stable and match server data.
- **Player tracking (no new world every time)**:
  - Client: persistent `PlayerId` in `localStorage`; `IClientIdProvider` / `ClientIdProvider` supply it; sent as `ClientId` in create-session request.
  - Server: `CreateSessionRequest.ClientId`; `Session.ClientId` stored in DB; for SinglePlayer + clientId, server returns existing active single-player session for that client (and its world) instead of creating a new one. See [reference/architecture.md](../reference/architecture.md#player-tracking-single-player-resume).
