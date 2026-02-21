# API and transport

## SignalR hubs

### GameHub (`/commandhub`)

Methods map 1:1 to commands. Each takes `sessionId` (and command args), resolves `PlayerId` from `Context.UserIdentifier` (or Guid.Empty if not set), and calls `ICommandIngress.SubmitAsync`.

| Method | Parameters | Command |
|--------|------------|---------|
| MoveFleet | sessionId, fleetId, toSystemId, clientTick | MoveFleet |
| QueueBuild | sessionId, systemId, design, count, clientTick | QueueBuild |
| StartRally | sessionId, regionId, clientTick | StartRally |
| StartMartialLaw | sessionId, systemId, clientTick | StartMartialLaw |

### WorldHub (`/gamehub`)

- **JoinWorld(worldId)** — Add connection to group; optionally send full world.
- Server pushes **ReceiveUpdates** (deltas or event-shaped updates) to the session/world group.

## REST

- **Session lifecycle**: Create session, join session, get game state (unchanged).
- **Command endpoints** (optional, same pipeline as hub):
  - `POST /game/move-fleet` — body: MoveFleetEvent-like (playerId, fleetId, fromPlanetId, toPlanetId); query: worldId. Submits MoveFleet via ICommandIngress.
  - `POST /game/session/{sessionId}/commands/move-fleet` — body: MoveFleetCommandDto (playerId, clientTick, fleetId, toSystemId). Submits MoveFleet via ICommandIngress.

All command paths (SignalR and REST) go through ICommandIngress so the engine sees a single stream of commands.
