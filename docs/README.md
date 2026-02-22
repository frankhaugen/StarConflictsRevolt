# Star Conflicts Revolt — Documentation

All documentation lives under **docs/**, organized hierarchically. For a flat list see [INDEX.md](INDEX.md).

**First time here?** Start with [getting-started.md](getting-started.md) to run the stack, create a session, join, and send a move-fleet command.

---

## Quick start

| Step | Command |
|------|--------|
| **Build** | `dotnet build StarConflictsRevolt.slnx` (stop AppHost first if running) |
| **Run stack** | `dotnet run --project StarConflictsRevolt.Aspire.AppHost` (Docker required for DBs) |
| **Test** | `dotnet test StarConflictsRevolt.Tests` |

See [operations/development.md](operations/development.md) for details and [operations/aspire.md](operations/aspire.md) for AppHost and containers. Problems? See [operations/troubleshooting.md](operations/troubleshooting.md).

---

## Documentation hierarchy

### Entry and narrative (docs root)

| Document | Description |
|----------|-------------|
| [getting-started.md](getting-started.md) | **Getting started** — prerequisites, run stack, create session, join world, move fleet. |
| [story.md](story.md) | **The story** — vision, commands vs events, how a command becomes reality, world summary. |

### Reference (specs)

| Document | Description |
|----------|-------------|
| [reference/architecture.md](reference/architecture.md) | Pipeline, tick loop, event types, event store, SignalR/REST. |
| [reference/domain.md](reference/domain.md) | World vs session, map (systems, fleets, ETA), economy, loyalty. |
| [reference/api-transport.md](reference/api-transport.md) | Base URL and auth, client flow, WorldHub, GameHub, REST endpoints, examples. |
| [reference/encounters.md](reference/encounters.md) | Abstract encounter resolution (no tactical combat). |
| [reference/glossary.md](reference/glossary.md) | Definitions: command, event, session, world, tick, delta, hubs. |

### Operations (runbooks and development)

| Document | Description |
|----------|-------------|
| [operations/aspire.md](operations/aspire.md) | AppHost, resources (redis, gameDb, ravenDb), health checks, Blazor config. |
| [operations/development.md](operations/development.md) | Solution layout, build/test/run, where key code lives. |
| [operations/troubleshooting.md](operations/troubleshooting.md) | Build locked, containers unhealthy, 404s, no deltas, auth. |
| [operations/playtest-runbook.md](operations/playtest-runbook.md) | Assistant-driven playtest with Playwright MCP (create/join, galaxy). |
| [operations/playtesting-strategy.md](operations/playtesting-strategy.md) | Playtesting goals, levels, scenarios, what to observe. |

### Tooling and implementation

| Document | Description |
|----------|-------------|
| [tooling/agents.md](tooling/agents.md) | Copilot/AI agent instructions (Aspire, MCP, Playwright). |
| [tooling/tunit-playwright.md](tooling/tunit-playwright.md) | TUnit and Playwright integration for UI tests. |
| [tooling/implementation-plan.md](tooling/implementation-plan.md) | Implementation plan for Blazor, testing, diagnostics. |
| [tooling/implementation-summary.md](tooling/implementation-summary.md) | Summary of completed implementation. |

### Projects

| Document | Description |
|----------|-------------|
| [projects/clients-shared.md](projects/clients-shared.md) | StarConflictsRevolt.Clients.Shared project overview. |

### Work items

| Resource | Description |
|----------|-------------|
| [work-items/README.md](work-items/README.md) | How to use work items — format, creating, moving to done. |
| [work-items/current.md](work-items/current.md) | Active work (to-do / in progress). |
| [work-items/done.md](work-items/done.md) | Done pile (completed with date). |

When you finish a task: move its block from **current** to **done**, add a **Completed:** date. See [work-items/README.md](work-items/README.md).

### Legacy

| Resource | Description |
|----------|-------------|
| [legacy/](legacy/) | Legacy design docs: ARCHITECTURE, BlazorClient, CombatSystem, MissionSystem, etc. Reference only; [reference/](reference/) defines the current backend. |

---

## Building and running

- **Solution:** `StarConflictsRevolt.slnx` at repo root. Build: `dotnet build StarConflictsRevolt.slnx`. Stop any running AppHost first if the build reports a locked exe.
- **Local orchestration:** [operations/aspire.md](operations/aspire.md) — AppHost runs dashboard, webapi, Blazor, and (by default) containerized Redis, SQL Server, RavenDB. Set `Aspire:UseContainers` to `false` to use existing DBs and avoid starting containers.
