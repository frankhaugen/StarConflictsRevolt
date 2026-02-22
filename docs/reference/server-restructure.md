# Server project restructure

## Overview

The simulation engine is extracted into its own project with clear edges:

- **StarConflictsRevolt.Server.Simulation** – Simulation engine and domain (no HTTP, no SignalR).
- **StarConflictsRevolt.Server.WebApi** – Host, API, application services; references Simulation.

## Simulation project (`StarConflictsRevolt.Server.Simulation`)

### Contents

- **Domain** – Full game domain: commands, events, world model (World, Galaxy, StarSystem, Planet, Fleet, etc.), enums, AI/Combat/Missions types. Base types: `GameObject` (record) for record-based entities, `GameObjectBase` (class) for class-based entities (Gameplay.World, Gameplay.Session, etc.).
- **Domain.IPlayerController** – Abstraction for players (human or AI) that generate commands; implemented by WebApi’s `PlayerController`.
- **Engine** – Tick loop and sim execution:
  - `GameTickService` – Publishes `GameTickMessage` every 100 ms via Frank.PulseFlow (or ITickPublisher).
  - `IGameSim` / `GameSim` – Command → events (e.g. MoveFleet → FleetOrderAccepted).
  - `GameTickMessage`, `GameTickNumber`, `GameTimestamp`, `GameSessionId`, `QueuedCommand`, **`ICommandQueue`** (interface: `TryEnqueue`, `DrainAsync`). Simulation defines the contract only; the host provides the implementation (e.g. channel-based queue).

### Dependencies

- `StarConflictsRevolt.Server.EventStorage.Abstractions` (IGameEvent)
- `Frank.PulseFlow`
- `Microsoft.Extensions.Hosting.Abstractions`, `Microsoft.Extensions.Logging.Abstractions`

No ASP.NET, no SignalR.

## WebApi project

- References **Simulation** and uses Simulation.Domain / Simulation.Engine for world state, commands, events, and tick types.
- **Application** layer: `WorldEngine`, `GameUpdateService`, `SessionAggregateManager`, `SessionAggregate`, **queues** (see below), `PlayerController` (implements `IPlayerController`), hubs, AI/combat services, etc.
- **Queues that must exist** (see [architecture.md](architecture.md#queues-that-must-exist)): (1) **ICommandQueue** → `CommandQueueChannel` (single channel, single reader = WorldEngine); (2) **CommandQueue** (legacy per-session queues, drained by GameUpdateService); (3) event store’s internal channel (inside RavenEventStore). All registered in StartupHelper / host.
- **API** – Endpoint handlers, auth, session, game actions.
- **Infrastructure** – Configuration, LiteDb persistence, MessageFlows (e.g. `GameTickMessageFlow` consuming ticks and calling `GameUpdateService` / `AiTurnService`).

## Current duplication

- **WebApi** still contains **Core/Domain** (persistence-oriented types under `Core.Domain.Gameplay` and related). These are used by LiteDb and `EntityExtensions` for mapping. Migration path: switch persistence and mappings to Simulation.Domain types and then remove WebApi’s Core/Domain.

## Registration

- `StartupHelper` registers `IGameSim` → `GameSim` (Simulation), `ICommandQueue` → `CommandQueueChannel`, `GameTickService` (Simulation), and the rest of the application services and hubs.
