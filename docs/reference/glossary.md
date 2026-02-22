# Glossary

| Term | Definition |
|------|------------|
| **Command** | Player intent (MoveFleet, QueueBuild, …). From clients; not stored. May be rejected. |
| **Event** | Fact from sim (FleetOrderAccepted, FleetArrived, CommandRejected, …). Persisted. |
| **Session** | One game (GUID). One world, one SessionAggregate. Create/join via REST. |
| **World** | Full state: Galaxy, fleets, players. In SessionAggregate; event-sourced. |
| **WorldId** | = sessionId. Group key for JoinWorld and REST. |
| **Tick** | Discrete step (~10/s). Drain commands → sim → time advancement → persist → push deltas. |
| **EtaTick** | Tick when fleet arrives. FleetArrived when currentTick ≥ EtaTick. |
| **Delta** | World-state change. Pushed via ReceiveUpdates. |
| **Ingress** | ICommandIngress: enqueue commands. Queue: ICommandQueue, drained each tick. |
| **Engine** | WorldEngine.TickAsync. Sim: IGameSim — command → events. |
| **Aggregate** | SessionAggregate: world + Apply(event). Event store: RavenEventStore. |
| **WorldHub** | /gamehub. JoinWorld(worldId); ReceiveUpdates. |
| **GameHub** | /commandhub. Command methods → ICommandIngress. |
| **Authoritative** | Server is truth; client sends commands, applies server deltas. |
| **Galaxy** | Star systems (nodes + edges). Fleet: owner, location, Status (Idle/Moving), EtaTick. |
