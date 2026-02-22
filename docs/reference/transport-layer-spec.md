# Transport layer specification

Specification for the **Transport** layer: a single fan-out for game ticks (and optional signals) to **in-process listeners** and **SignalR**, while keeping the **Simulation** decoupled from transport and from domain-aware consumers.

## Goals

1. **Single tick source** — One producer (ticker) publishes ticks; Transport delivers to both in-process consumers and SignalR.
2. **Simulation decoupled** — Simulation (engine, sim, domain) does not depend on SignalR, PulseFlow, or any concrete transport. It reacts to ticks via an abstraction.
3. **Domain at the edges** — Transport moves **domain-agnostic** payloads (tick number, timestamp). Listeners/consumers that care about domain (sessions, world, etc.) live in the application layer and use domain services when handling a tick.
4. **Testability** — Ticker and simulation can be tested with a fake transport; listeners can be tested with a fake tick source.

## Boundaries

| Layer | Responsibility | Depends on |
|-------|----------------|------------|
| **Ticker** | Emit ticks at fixed interval (e.g. 10/s). Calls Transport to publish. | `ITickPublisher` only |
| **Transport** | Receive tick from Ticker; fan out to in-process listeners and SignalR. | Listeners contract, SignalR (in host only) |
| **Simulation** | Execute commands, advance world, produce events. Invoked by a listener with a tick. | Domain (World, Commands, Events); **not** Transport or SignalR |
| **Listeners** | In-process consumers of ticks. One or more; may use domain services (WorldEngine, SessionAggregateManager). | Transport contract (tick DTO); domain services |
| **SignalR** | Push tick (and other client payloads) to connected clients. | Transport calls into hub context |

**Simulation is not a worker.** The only “clock” worker is the Ticker. Simulation is a **consumer** of the tick stream, invoked by a listener that the Transport calls.

## Contracts (interfaces)

### 1. Tick payload (domain-agnostic)

Transport and all consumers use a small, domain-free DTO. No World, Session, or game types.

- **TickNumber** (long) — monotonic tick index.
- **Timestamp** (DateTime UTC) — when the tick was produced.

This can be the existing `GameTickMessage` (or a slimmer `TickSignal` record) living in **Simulation.Engine** or in a small **Transport.Contracts** project that has no reference to SignalR or hosting. Simulation already has `GameTickNumber` and `GameTimestamp`; the payload is just those two plus any envelope id needed for tracing.

### 2. Producer side (Ticker → Transport)

**ITickPublisher** (implemented by Transport; consumed by Ticker)

```csharp
Task PublishTickAsync(TickSignal tick, CancellationToken cancellationToken);
```

- **Ticker** (e.g. `GameTickService`) depends only on `ITickPublisher`. It creates a tick payload and calls `PublishTickAsync`. No IConduit, no SignalR.
- **Transport** implementation: on `PublishTickAsync`, (1) notify all registered in-process listeners, (2) push the tick to SignalR (e.g. per-session or broadcast).

`TickSignal` is the minimal type (tick number + timestamp); it can alias or wrap the same data as `GameTickMessage` if we keep that name for backward compatibility.

### 3. In-process consumption (Transport → Listeners)

**ITickListener** (implemented by app-layer consumers; registered with Transport)

```csharp
Task OnTickAsync(TickSignal tick, CancellationToken cancellationToken);
```

- **Transport** holds a list of `ITickListener`. When a tick is published, it awaits each listener (order can be specified: e.g. “game update” first, then “diagnostics”).
- **Listeners** are registered at startup (DI). Examples:
  - **Game update listener** — calls WorldEngine.TickAsync, then BroadcastTickToActiveSessionsAsync, then ProcessAllSessionsAsync (or a single orchestrator service that does this). This listener depends on SessionAggregateManager, WorldEngine, IHubContext, etc.; Transport does not.
  - **AI listener** (if separate) — calls AiTurnService.ProcessTickAsync.
  - **Diagnostics / metrics listener** — records tick rate, latency.

Simulation types (WorldEngine, IGameSim) are **not** listeners themselves. A listener in the **application layer** receives the tick and invokes WorldEngine (and other domain services). So Simulation stays decoupled from “how the tick arrived.”

### 4. SignalR (Transport → Clients)

- Transport implementation calls `IHubContext<WorldHub>.Clients.Group(sessionId).SendAsync("ReceiveTick", tick.TickNumber)` (and/or All if no session). Same contract as today for clients.
- No domain types on the wire; only tick number (and optionally timestamp) as defined in [api-transport.md](api-transport.md).

## Data flow

```mermaid
flowchart TB
  subgraph Producer
    Ticker[GameTickService / Ticker]
  end

  subgraph Transport
    Pub[ITickPublisher impl]
    Reg[Registered ITickListener list]
  end

  subgraph InProcess
    L1[Game update listener]
    L2[AI listener]
    L3[Other listeners]
  end

  subgraph Simulation
    WorldEngine[WorldEngine]
  end

  subgraph SignalR
    Hub[WorldHub]
  end

  subgraph Clients
    Blazor[Blazor / clients]
  end

  Ticker -->|PublishTickAsync| Pub
  Pub --> Reg
  Reg --> L1
  Reg --> L2
  Reg --> L3
  L1 -->|TickAsync(tick)| WorldEngine
  Pub -->|SendAsync ReceiveTick| Hub
  Hub --> Blazor
```

- **Ticker** → **Transport** (single publish).
- **Transport** → **In-process**: calls each **ITickListener.OnTickAsync(tick)**. One of those listeners uses **WorldEngine** and other domain services; Simulation does not reference Transport or SignalR.
- **Transport** → **SignalR**: same tick (tick number, optional timestamp) pushed to clients.

## Where types live

| Type | Project / layer | Notes |
|------|------------------|--------|
| **TickSignal** / **GameTickMessage** | Simulation.Engine (or Transport.Contracts) | Domain-agnostic; no SignalR ref |
| **ITickPublisher** | Simulation.Engine or Transport.Contracts | So Ticker can depend on it without depending on WebApi |
| **ITickListener** | Same as ITickPublisher or WebApi | Implemented by app layer |
| **Transport implementation** | WebApi (or dedicated Transport host project) | Implements ITickPublisher; registers listeners; uses IHubContext |
| **Ticker** | Simulation (BackgroundService) | Depends only on ITickPublisher |
| **WorldEngine, IGameSim** | Simulation / WebApi | No dependency on Transport or SignalR; invoked by a listener |
| **Game update / AI listeners** | WebApi | Depend on WorldEngine, SessionAggregateManager, IHubContext, etc. |

If **Simulation** must not reference any host-specific contract, put **ITickPublisher** and **TickSignal** in a small **Transport.Contracts** (or **Simulation.Engine**) project that both Simulation and WebApi reference. Transport implementation and **ITickListener** can live in WebApi so that listeners can inject IHubContext and domain services.

## Listener registration

- At startup, the host (WebApi) registers all **ITickListener** implementations with the Transport (e.g. via DI: the Transport receives `IEnumerable<ITickListener>`).
- Order of invocation can be defined (e.g. game update first, then AI, then diagnostics) so that simulation and broadcast happen before side effects.

## Error and backpressure

- If a listener throws, Transport can catch, log, and either continue to other listeners or fail the publish (specify desired behavior).
- SignalR send should not block in-process listeners; fire-and-forget or non-blocking push after listeners run is acceptable. Optionally run listener phase and SignalR phase in parallel (listeners first, then SignalR) to avoid tick processing waiting on network.

## Queues and transport

Transport only delivers **ticks**. Command flow uses separate queues that must exist in the host (see [architecture.md](architecture.md#queues-that-must-exist)):

- **ICommandQueue** (channel): Commands from Ingress (hub/REST) and AI are enqueued here; the game-update listener calls **WorldEngine.TickAsync**, which **drains** this queue at each tick. The tick triggers the drain; it does not carry commands.
- **CommandQueue** (legacy per-session): REST game-action handlers may enqueue here; the same game-update flow drains it per session. Both queues can coexist.
- **Event store channel**: Internal to the event store; buffers events from publish to persist/broadcast.

Listeners that run on each tick must have access to **ICommandQueue** (and optionally CommandQueue) and **WorldEngine** so that commands are drained and simulation/deltas run. Transport does not define these queues; the host wires them.

## Summary

- **Transport** = single fan-out: one **ITickPublisher** implementation that, on **PublishTickAsync(tick)**, notifies all **ITickListener**s and pushes the same tick to **SignalR**. Payload is domain-agnostic (tick number + timestamp).
- **Ticker** lives in an independent worker (e.g. Simulation as a library, host runs it); it only depends on **ITickPublisher**.
- **Simulation** is decoupled: it never references Transport or SignalR; a **listener** in the app layer receives ticks and calls WorldEngine (and other domain services). Listeners are the only place that combine “tick” with domain types (sessions, world, hub).
- **SignalR** and **in-process** both react to the same tick from the same publish call; clients get ReceiveTick, and server-side logic runs via ITickListener.
- **Queues**: Command processing relies on **ICommandQueue** (and optionally a per-session CommandQueue) and the event store internal channel; these are host concerns, not part of the Transport contract. See [architecture.md](architecture.md#queues-that-must-exist).
