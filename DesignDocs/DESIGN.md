# Game Design Document

## Star Conflicts: Revolt

A real-time turn-based-ish 4X strategy game inspired by *Star Wars: Rebellion*.

## 📘 Overview

A turn-based 4X strategy game inspired by *Star Wars: Rebellion*.  
Written in C#/.NET, with a backend using Aspire, SignalR, and RavenDB, and frontends via Raylib-CSharp or Blazor.

---

## 🚀 Architecture Summary

```text

StarConflictsRevolt/
├── src/
│   ├── StarConflictsRevolt.Dtos           # Shared DTOs
│   ├── StarConflictsRevolt.Client         # Client library (NuGet)
│   ├── StarConflictsRevolt.ApiService     # API + SignalR Hub
│   ├── StarConflictsRevolt.EngineWorker   # Simulation engine & eventing
│   ├── StarConflictsRevolt.Store.Eventing # RavenDB event store
│   └── StarConflictsRevolt.AppHost        # Aspire orchestrator
└── StarConflictsRevolt.sln

```

- Client & DTOs packaged as NuGet.
- Aspire `AppHost` spins up Redis or RavenDB, API, Engine simultaneously.

---

## 📦 Shared DTOs

```csharp
public record WorldDto(Guid WorldId, List<PlanetDto> Planets);
public record PlanetDto(Guid Id, string Name, ...)
public record GameObjectUpdate(Guid Id, UpdateType Type, JsonElement? Data);
public enum UpdateType { Added, Changed, Removed }
````

---

## 🧩 Domain Model

```csharp
abstract record GameObject { Guid Id = Guid.CreateVersion7(); }

record World(...)   : GameObject { GameSpeed Speed; Galaxy Galaxy; List<Player> Players; }
record Galaxy(...)  : GameObject { List<SystemRegion> Systems; }
record SystemRegion : GameObject { List<Planet> Planets; }
record Planet(...)  : GameObject { List<Fleet> Fleets; }

record Player(Guid Id, string Name, PlayerController Controller) : GameObject;

abstract record PlayerController { Guid PlayerId; }
record HumanController(Guid PlayerId, string ConnectionId) : PlayerController;
record AiController(Guid PlayerId) : PlayerController;
```

---

## 🤖 AI Design

* AI implements `IPlayerController`
* Uses utility scoring or HTN/GOAP to generate `CommandDto`
* Runs internally in EngineWorker, identical to human logic
* Keeps parity and fairness between AI and human players

---

## 🧪 Change Tracking & Delta Updates

```csharp
public class ChangeTracker<T> where T: GameObject
{
  // JSON diff-based delta tracking
  public IEnumerable<GameObjectUpdate> Track(IEnumerable<T> current) { … }
}
```

* Throttled (\~200ms), scoped by world via SignalR groups
* Client applies deltas to local `WorldDto` state

---

## 🧰 Client Side Structure

```csharp
public interface IClientWorldStore {
  void ApplyFull(WorldDto);
  void ApplyDeltas(IEnumerable<GameObjectUpdate>);
  WorldDto GetCurrent();
  IReadOnlyList<WorldDto> History { get; }
}

public interface IGameRenderer {
  Task RenderAsync(WorldDto world, CancellationToken ct);
}
```

* `ClientWorldStore` handles snapshots with history buffer
* `GameClient` manages SignalR connections, world join, and ties into `IGameRenderer`

---

## 🧵 Event Store with RavenDB

```csharp
public record EventEnvelope(Guid WorldId, IGameEvent Event, DateTime Timestamp);
public class RavenEventStore : IEventStore { … }
```

* Single-threaded channel writer ensures ordered persistence
* Subscribers (e.g., SignalR broadcaster) notified after save
* Optimistic concurrency and graceful shutdown

---

## 🔐 Persistence & Concurrency

* RavenDB sessions are single-threaded
* Use optimistic concurrency + retry logic
* Option to use Compare-Exchange or lock-documents for distributed locks

---

## 🔁 Replay & AI Learning

* All meaningful actions are stored as **events** (not raw commands)
* Enables full game replay, auditing, and AI experience replay via event logs

---

## 🧯 Scaling & Resilience

* SignalR uses Redis backplane (configured via `AddStackExchangeRedis(...)`)
* Throttled updates via SignalR Groups
* Aspire orchestrates local containers (Redis, RavenDB) and services
* Supports horizontal scaling, reconnection logic, burst resiliency

---

## 🧠 Naming Conventions

Pattern: `StarConflictsRevolt.Function.Component`

E.g.:

* `StarConflictsRevolt.Store.Eventing.RavenEventStore`
* `StarConflictsRevolt.ApiService.GameHub`
* `StarConflictsRevolt.EngineWorker.ChangeTracker`

---

## 👍 Sustainability & Benefits

* Delta-based updates reduce bandwidth for late-game thousands of objects
* Eventing + RavenDB provide replayability, AI training, and audit trail
* AI runs inside engine without backdoors and shares command pipeline
* Client library enables multiple renderer choices (Raylib, Blazor)
* Aspire ensures consistent local/cloud setup with infrastructure containers

---

## 🔜 Next Steps

* Flesh out concrete `IGameEvent` types and event subscribers (SignalR, read models)
* Scaffold `IGameRenderer` stub for Raylib or Blazor Canvas
* Add AI logic prototype (Utility AI)
* Add snapshotting logic and replay tooling
* Configure CI for NuGet packaging and Aspire deployment

---

## 🔗 Code Snippets

* Renderer loop, change tracker, SignalR wiring, event pushing included in their relevant sections above as prototypes from this conversation.

---

*End of Document*

```

Let me know if you'd like this expanded with CI scripts, event schemas, or a more formal design spec!
```
