# Star Conflicts Revolt

Event-sourced, real-time 4X strategy backend and Blazor client—inspired by *Sins of a Solar Empire* and *Star Wars Rebellion*. Single-process server: commands → simulation → events → SignalR deltas.

## Quick start

```bash
# Build (stop AppHost first if running)
dotnet build StarConflictsRevolt.slnx

# Run full stack (Docker required for Redis, SQL Server, RavenDB)
dotnet run --project StarConflictsRevolt.Aspire.AppHost

# Tests
dotnet test StarConflictsRevolt.Tests
```

Open the Aspire dashboard URL from the console; use it to open the Blazor app and webapi.

## Project structure

| Project | Purpose |
|--------|---------|
| **StarConflictsRevolt.Server.WebApi** | Backend: REST + SignalR, event-sourced world, tick-driven sim (10 ticks/s). |
| **StarConflictsRevolt.Clients.Blazor** | Web client (Blazor Server); real-time updates via WorldHub. |
| **StarConflictsRevolt.Clients.Shared** | Shared HTTP/SignalR, auth, configuration. |
| **StarConflictsRevolt.Clients.Models** | DTOs for API and world state. |
| **StarConflictsRevolt.Aspire.AppHost** | Local orchestration: dashboard, webapi, Blazor, optional Redis/SQL/RavenDB containers. |
| **StarConflictsRevolt.Aspire.ServiceDefaults** | Shared Aspire defaults (health, OTLP). |
| **StarConflictsRevolt.Tests** | Unit and integration tests (TUnit). |

## Features

- **Event sourcing** — World changes are events in RavenDB; snapshots for fast load.
- **Tick loop** — Fixed 10 ticks/s: commands drained each tick, fleet arrivals advanced every tick.
- **Single pipeline** — Ingress → ICommandQueue → WorldEngine → event store → SignalR deltas.
- **Client-agnostic** — Same API for Blazor, REST, or other clients.

## Documentation

All specs and runbooks live in **[docs/](docs/README.md)**. New to the repo? Start with **[docs/getting-started.md](docs/getting-started.md)**.

| Doc | Description |
|-----|-------------|
| [docs/README.md](docs/README.md) | **Documentation hub** — quick start, hierarchy, work items. |
| [docs/getting-started.md](docs/getting-started.md) | **Getting started** — run stack, create session, join, move fleet. |
| [docs/story.md](docs/story.md) | Vision, commands vs events, from zero to running. |
| [docs/reference/](docs/reference/) | **Reference** — architecture, domain, api-transport, encounters, glossary. |
| [docs/operations/](docs/operations/) | **Operations** — aspire, development, troubleshooting, playtesting. |
| [docs/tooling/](docs/tooling/) | **Tooling** — agents, tunit-playwright, implementation plan/summary. |

**All documentation lives in [docs/](docs/README.md).** No other doc folders at solution root.
