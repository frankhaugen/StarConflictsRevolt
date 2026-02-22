# Reference (specs)

Technical specifications for the game backend.

| Document | Description |
|----------|-------------|
| [architecture.md](architecture.md) | Pipeline, tick loop, event types, event store, SignalR/REST. |
| [transport-layer-spec.md](transport-layer-spec.md) | Transport layer: tick fan-out to in-process listeners and SignalR; Simulation decoupled. |
| [domain.md](domain.md) | World vs session, map, fleets, economy, loyalty. Domain types live in **StarConflictsRevolt.Server.Domain**; Simulation has no domain models. |
| [api-transport.md](api-transport.md) | Auth, client flow, WorldHub, GameHub, REST, examples. |
| [encounters.md](encounters.md) | Abstract encounter resolution. |
| [glossary.md](glossary.md) | Definitions: command, event, session, world, tick, delta, hubs. |
| [storage-abstractions.md](storage-abstractions.md) | Repository contracts, IStorageBuilder, type-specific provider binding. |

See [../README.md](../README.md) for the full documentation map.
