# Development guide

Build, test, run, and where to find key code in the solution.

---

## Solution layout

| Path | Contents |
|------|----------|
| **StarConflictsRevolt.slnx** | Solution file (root). |
| **StarConflictsRevolt.Server.WebApi/** | Backend: handlers, domain, application services, infrastructure. |
| **StarConflictsRevolt.Clients.Blazor/** | Blazor Server UI. |
| **StarConflictsRevolt.Clients.Shared/** | Shared client libs (SignalR, HTTP, auth). |
| **StarConflictsRevolt.Clients.Models/** | DTOs. |
| **StarConflictsRevolt.Aspire.AppHost/** | Aspire orchestration (dashboard, projects, optional containers). |
| **StarConflictsRevolt.Aspire.ServiceDefaults/** | Shared Aspire defaults. |
| **StarConflictsRevolt.Tests/** | TUnit tests (unit + integration). |
| **docs/** | All documentation (this folder). Legacy design docs are in [../legacy/](../legacy/). |

---

## Build and test

| Action | Command |
|--------|--------|
| **Build solution** | `dotnet build StarConflictsRevolt.slnx` |
| **Build one project** | `dotnet build StarConflictsRevolt.Server.WebApi/StarConflictsRevolt.Server.WebApi.csproj` |
| **Run tests** | `dotnet test StarConflictsRevolt.Tests` |
| **Run tests (filter)** | `dotnet test StarConflictsRevolt.Tests --filter "FullyQualifiedName~GameTick"` |

**Build note:** If the Aspire AppHost is running, the build can fail with “file is locked” (MSB3027). Stop the AppHost, then build again.

---

## Running the application

| Mode | Command | Notes |
|------|--------|------|
| **Full stack (Aspire)** | `dotnet run --project StarConflictsRevolt.Aspire.AppHost` | Dashboard, webapi, Blazor; Docker required for Redis/SQL/RavenDB unless you set `Aspire:UseContainers=false`. |
| **WebApi only** | `dotnet run --project StarConflictsRevolt.Server.WebApi` | Needs connection strings (gameDb, ravenDb, redis) in config or env. |
| **Blazor only** | `dotnet run --project StarConflictsRevolt.Clients.Blazor` | Needs `GameClientConfiguration__*` and token endpoint pointing at webapi. |

See [aspire.md](aspire.md) (this folder) for AppHost resources, health checks, and container options.

---

## Where key code lives (Server.WebApi)

### Tick loop and simulation

| Responsibility | Location |
|----------------|----------|
| **Tick publisher** | `Application/Services/Gameplay/GameTickService.cs` — BackgroundService; publishes `GameTickMessage` every ~100 ms via Frank.PulseFlow. |
| **Tick handler** | `Infrastructure/MessageFlows/GameTickMessageFlow.cs` — Handles each tick: AiTurnService, then GameUpdateService. |
| **Game update / engine driver** | `Application/Services/Gameplay/GameUpdateService.cs` — Calls WorldEngine.TickAsync, then processes legacy CommandQueue per session. |
| **World engine** | `Application/Services/Gameplay/WorldEngine.cs` — TickAsync: drain commands, run sim, apply events; then time-advancement (fleet arrivals). |
| **Simulation logic** | `Application/Services/Gameplay/GameSim.cs` — IGameSim.Execute (e.g. MoveFleet → FleetOrderAccepted). |
| **Fleet arrivals** | `Core/Domain/Events/FleetArrived.cs` — Applied when tick ≥ fleet EtaTick. |

### Commands and events

| Responsibility | Location |
|----------------|----------|
| **Command ingress** | `Application/Services/Gameplay/CommandIngress.cs` — ICommandIngress; enqueues to ICommandQueue. |
| **Command queue (tick drain)** | `Application/Services/Gameplay/CommandQueueChannel.cs` — ICommandQueue; DrainAsync used by WorldEngine. |
| **Session/world state** | `Application/Services/Gameplay/SessionAggregateManager.cs`, `SessionAggregate.cs` — GetOrCreateAggregate, Apply(event). |
| **Event store** | `Core/Domain/Events/RavenEventStore.cs` — Persists EventEnvelope; snapshots. |
| **Event broadcast** | `Application/Services/Gameplay/EventBroadcastService.cs` — Subscribes to store; pushes ReceiveUpdates to SignalR groups. |

### API surface

| Responsibility | Location |
|----------------|----------|
| **Endpoint registration** | `API/Handlers/Endpoints/ApiEndpointHandler.cs` — MapAllEndpoints; calls each handler’s MapEndpoints. |
| **Session (create, join, list)** | `API/Handlers/Endpoints/SessionEndpointHandler.cs` — POST /game/session, POST join, GET sessions/session. |
| **Game actions (move, build, attack, diplomacy)** | `API/Handlers/Endpoints/GameActionEndpointHandler.cs` — MoveFleet (via ICommandIngress), Build, Attack, Diplomacy (via CommandQueue). |
| **Auth** | `API/Handlers/Endpoints/AuthEndpointHandler.cs` — POST /token. |
| **Health** | `API/Handlers/Endpoints/HealthEndpointHandler.cs` — /, /health, /health/game. |
| **Hubs** | `Application/Services/Gameplay/WorldHub.cs` (MapHub /gamehub), GameHub (MapHub /commandhub) — registered in StartupHelper.ConfigureAsync. |

### Configuration and startup

| Responsibility | Location |
|----------------|----------|
| **DI and pipeline** | `Infrastructure/Configuration/StartupHelper.cs` — RegisterAllServices, RegisterRavenDb, RegisterLiteDb, ConfigureAsync; MapHub, MapEndpoints. |

---

## See also

- [../reference/architecture.md](../reference/architecture.md) — Pipeline and tick loop in detail.
- [../reference/api-transport.md](../reference/api-transport.md) — Hub methods and REST endpoints.
- [aspire.md](aspire.md) — AppHost and local run.
