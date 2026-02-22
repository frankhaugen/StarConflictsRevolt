# The story of StarConflictsRevolt

A short narrative of what this project is, why it’s built this way, and how the pieces fit together.

---

## The vision

**StarConflictsRevolt** is a **client-agnostic** sci-fi strategy backend. Think *Sins of a Solar Empire* meets *Star Wars Rebellion* (1998): you control fleets and systems on a map, you build and move, and when fleets meet the outcome is resolved in an abstract way—no tactical RTS micro, just strategic decisions and a “football manager” style resolution.

The backend doesn’t care whether the client is Blazor, a native app, or something else. It exposes **commands** (what the player wants to do) and **events** (what actually happened). The client sends commands and listens for updates; the server is the single source of truth.

---

## Why commands and events?

We separate **commands** from **events** on purpose.

- A **command** is *intent*: “Move this fleet here,” “Queue this build,” “Start martial law in this system.” Commands come from clients and are not stored as facts. They might be rejected (invalid move, not your fleet, etc.).
- An **event** is *fact*: “Fleet order accepted,” “Build queued,” “Encounter resolved: attacker won.” Events are produced by the simulation and **are** persisted. They form the history of the game.

So: the client says what it wants; the server decides what really happens and records only that. Replay, debugging, and multiple clients all align to the same event stream.

---

## How a command becomes reality

1. **You (the client)** call the server: “Move fleet X to system Y,” via SignalR (GameHub) or REST. That’s a **command**.
2. **Ingress** checks it quickly (session valid, IDs present) and enqueues it. No game logic yet.
3. On the next **tick**, the **engine** drains the command queue, groups by session, and runs the **simulation** for each command.
4. The sim validates against the current **world** (can this fleet move there? do you own it?). It emits **events** (e.g. FleetOrderAccepted or CommandRejected).
5. Events are **persisted** to the event store and **pushed** to connected clients via SignalR. The world state is updated; clients get deltas and stay in sync.

All of this runs in one process (the WebApi). No separate worker; the tick loop drives the engine, and the same app serves HTTP and SignalR. Simple to run and reason about.

---

## The world in a nutshell

- **Map**: Systems (nodes) with industry, loyalty, defenses, connected by a graph. One galaxy per world.
- **Fleets**: Owner, location (system or in transit), power, ETA. Movement is abstract—no real-time steering, just “arrives at tick N.”
- **Economy**: Industry produces credits and shipyard capacity; you spend to build fleets. Factions have credits and build slots.
- **Encounters**: When hostile fleets are in the same system, the server resolves the clash from power (and optional variance): Win / Stalemate / Loss. No tactical layer in the minimal slice.

Loyalty and martial law add simple modifiers (output up/down, compliance). We deliberately keep the first slice minimal: no Death Star runs, no mission trees, no deep diplomacy. Get the loop right—tick, economy, movement, encounters, events—then extend.

---

## How you talk to the server

- **WorldHub** (`/gamehub`): You join a world; the server sends you the full state and then pushes **deltas** as the game evolves.
- **GameHub** (`/commandhub`): You send commands here—MoveFleet, QueueBuild, StartRally, StartMartialLaw—each mapped 1:1 to a command type. They all go through the same **ingress** and queue.
- **REST**: Create/join sessions, get game state; optional REST endpoints for commands (e.g. move-fleet) that feed the same pipeline.

So: one pipeline (Ingress → Queue → Engine), multiple entry points. Clients can be Blazor today and something else tomorrow.

---

## From zero to running

1. **Build**: `dotnet build StarConflictsRevolt.slnx`. If the Aspire AppHost is running, stop it first or the build may report a locked exe.
2. **Run the stack**: `dotnet run --project StarConflictsRevolt.Aspire.AppHost`. You need **Docker** (Redis, SQL Server, RavenDB run as containers). The dashboard URL appears in the console; open it to see the webapi, Blazor app, and resources.
3. **Play**: Open the Blazor app from the dashboard, create or join a session, and send commands. The server will tick, persist events, and push updates back.

If you skip Docker, the webapi still starts but will warn about missing DB connection strings. Set `Aspire:UseContainers=false` and provide connection strings to use existing DBs; see [operations/aspire.md](operations/aspire.md). **First time?** Follow [getting-started.md](getting-started.md) for a step-by-step: create session, join world, send move-fleet.

---

## Where to read more

| Doc | What you get |
|-----|----------------|
| [getting-started.md](getting-started.md) | Step-by-step: run stack, create session, join, move fleet. |
| [reference/architecture.md](reference/architecture.md) | Pipeline, tick loop, event types, diagram. |
| [reference/domain.md](reference/domain.md) | Map, economy, fleet movement, World vs session. |
| [reference/api-transport.md](reference/api-transport.md) | Hubs, REST, auth, client flow, examples. |
| [reference/encounters.md](reference/encounters.md) | How encounters are resolved. |
| [operations/aspire.md](operations/aspire.md) | AppHost, resources, health checks, tuning. |
| [reference/glossary.md](reference/glossary.md) | Definitions: command, event, session, world, tick, delta. |
| [operations/troubleshooting.md](operations/troubleshooting.md) | Build failures, unhealthy containers, 404s, no deltas. |

This document is the **story**; those are the **specs and runbooks**. Together they give you a clear picture of what StarConflictsRevolt is and how to work with it.
