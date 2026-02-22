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
