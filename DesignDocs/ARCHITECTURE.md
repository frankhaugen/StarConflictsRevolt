### Star Conflicts Revolt – End-to-End Architectural Overview

*(high-level narrative of the moving parts and how they talk to each other; no code snippets)*

---

#### 1. Big-Picture “Slices”

| Slice                     | Purpose                                        | Typical Tech                               | Runs in                              |
| ------------------------- | ---------------------------------------------- | ------------------------------------------ | ------------------------------------ |
| **Presentation**          | Visualise the galaxy and capture player intent | Blazor (web) or Bliss (Veldrid-powered) desktop client | Player devices                       |
| **Connectivity**          | Real-time, low-latency duplex transport        | ASP.NET Core SignalR + optional plain REST | `StarConflictsRevolt.Server.WebApi`  |
| **Domain Simulation**     | Deterministic world state, AI, rules engine    | .NET worker process                        | `StarConflictsRevolt.EngineWorker`   |
| **Persistence & Replay**  | Immutable event log + periodic snapshots       | RavenDB                                    | `StarConflictsRevolt.Server.WebApi`  |
| **Orchestration & Infra** | Local/dev parity and cloud bootstrapping       | .NET Aspire AppHost, Redis, containers     | `StarConflictsRevolt.Aspire.AppHost` |

Everything is packaged as a single *solution* that can scale out horizontally: multiple EngineWorkers can own different “world partitions”, while the WebApi layer is stateless and fan-outs through a Redis back-plane.

---

#### 2. Runtime Interaction Flow (“one tick”)

```
┌────────────┐    ① CommandDto        ┌───────────────────┐      ② enqueue 
│   Client   │ ─────────────────────▶ │   WebApi          │ ═════════════════╗
└────────────┘                        └───────────────────┘                 ║
       ▲                                    │                              ▼
       │   ⑤ WorldDto + delta               │ ③ Channel<TCommand>      ┌──────────────┐
       └─────────────────────────────────────┴────────────────────────▶ │ EngineWorker │
                                            ▲                           └──────────────┘
                                            │ ④ EventEnvelope           ║
                                            │   (persist)               ▼
                                     ┌───────────────────┐        RavenDB Event-log
                                     │ RavenEventStore   │
                                     └───────────────────┘
```

1. **Human or AI** generates a `CommandDto` (move fleet, start construction, etc.) and sends it via the client SDK.
2. `WebApi` enqueues the command on a **single-writer channel** that the owning EngineWorker aggressively polls.
3. **EngineWorker** validates and executes the command inside its authoritative simulation loop (default 5 Hz).
4. Every *meaningful mutation* is wrapped in an `EventEnvelope` and atomically appended to RavenDB.
5. After processing the tick, EngineWorker runs `ChangeTracker<T>` to compute a *JSON diff* between the previous and new aggregate World snapshot. It pushes that lightweight delta back through SignalR **groups** (one group per world), which the client applies to its local copy.

Because the EngineWorker is the *sole* writer for its world, concurrency is simple; optimistic checks are only needed when multiple workers might touch the same RavenDB collection (e.g., cross-world leaderboard read models).

---

#### 3. The Event Log & Snapshots

* **Why events?** Allows deterministic replays, AI reinforcement-learning, auditability, and roll-backs.
* **Schema:** `EventEnvelope(Guid WorldId, IGameEvent Event, DateTime Timestamp)` (plus Raven metadata for etags).
* **Snapshots:** Every *n* events (configurable, default 500) a full `WorldDto` snapshot is saved so that catch-up time never explodes.
* **Subscriptions:**

  * *SignalR Broadcaster* – turns new events into `GameObjectUpdate` deltas.
  * *Read-Model Builders* – generate materialised views (e.g., leaderboards) asynchronously.

---

#### 4. Client-Side Responsibilities

| Concern              | Implementation                                                                                                                                |
| -------------------- | --------------------------------------------------------------------------------------------------------------------------------------------- |
| **State cache**      | `IClientWorldStore` keeps the latest `WorldDto` plus a ring-buffer of history for UX rewind or debugging.                                     |
| **Transport**        | Wrapper over SignalR that *transparently* transforms outgoing player actions into `CommandDto`s and incoming deltas into `GameObjectUpdate`s. |
| **Renderer plug-in** | Any module implementing `IGameRenderer` can consume `WorldDto` – e.g., a Blazor web UI or Bliss 2-D star-map.                                                  |
| **Offline support**  | Because every change is event-sourced, a thin “inspect & replay” tool can run entirely client-side for battle analysis.                       |

- **Blazor client** is the primary web-based UI implementation in this repository.
- **Bliss client** was previously implemented but has been removed in favor of the Blazor client.
- **Shared client logic** (HTTP, SignalR, auth, config) is in `StarConflictsRevolt.Clients.Shared`.
- **DTOs** for all API and world state are in `StarConflictsRevolt.Clients.Models`.

---

#### 5. API Structure

- **Handlers, not Controllers**: All API endpoints are organized by concern in modular Handler classes (see `ApiEndpointHandler`, `SessionEndpointHandler`, etc.).
- **No direct references from Http to Models**: Extension methods for IHttpApiClient belong in the client projects; DTOs are always in Models.

---

#### 6. Testing & UI Architecture

- **Testable UI**: All view logic is structured to be testable without requiring the actual renderer.
- **Dependency Injection**: All types in tests are resolved from DI, ensuring logging and other services are available.
- **Integration tests**: Use TUnit and a custom builder to run both server and client with in-memory fakes.

---

#### 7. Scaling & Deployment Story

* **Dev-first:** `StarConflictsRevolt.Aspire.AppHost` spins up RavenDB, Redis, EngineWorker, and WebApi locally with a single `dotnet run`.
* **Horizontal scaling:**

  * *SignalR* uses Redis back-plane for pub/sub; any number of WebApi replicas can sit behind a load-balancer.
  * *World sharding* – Kubernetes or Azure Container Apps schedule multiple EngineWorkers, each responsible for a deterministic range of WorldIds (e.g., modulo hash).
* **Resilience:**

  * EngineWorker writes are idempotent (events include version numbers).
  * Graceful shutdown by draining the command channel, flushing snapshots, and relinquishing world ownership via Compare-Exchange in RavenDB.
* **Observability:**

  * OpenTelemetry for traces; Prometheus metrics (tick duration, SignalR P95 latency, etc.).
  * A lightweight “World Inspector” Blazor page can attach to a running EngineWorker via gRPC stream for live debugging.

---

#### 8. Security & Integrity

* **GUID v7** identifiers—keeps ordering information while being globally unique, thwarting enumeration attacks.
* **Optimistic concurrency** on RavenDB sessions; if two background services attempt to write the same projection document, retries back-off with jitter.
* **Command validation** happens once—*before* it is enqueued—to reject impossible moves early and keep the event log pure.
* **Authentication/Authorisation** uses JWT (web) or Steam OpenID (desktop) with SignalR bearer tokens.

---

### Summary

Star Conflicts Revolt organises its codebase around a **single-writer, event-sourced core** protected behind a thin real-time API. Clients—whether Blazor web or future desktop/other clients—receive *only* deterministic deltas, apply them locally, and render however they like. All long-running work (simulation, AI, persistence) lives in back-end workers that the Aspire AppHost can spin up on a laptop or a cloud cluster with equal ease. The result is a modern, cloud-friendly re-imagining of *Star Wars Rebellion* that stays maintainable, scalable, and replayable from day one.
