# API and transport

All commands go through **ICommandIngress**. Base URL = webapi root. Auth: JWT in `Authorization: Bearer <token>`; get token via **POST /token**. Hubs: `{baseUrl}/gamehub`, `{baseUrl}/commandhub`.

## Client flow

1. Connect to **WorldHub** + **GameHub**.
2. **JoinWorld(worldId)** on WorldHub (use sessionId from create/join).
3. Subscribe to **ReceiveUpdates** on WorldHub (deltas when world changes) and **ReceiveTick** (simulation tick number every tick).
4. Send commands via GameHub or REST → outcomes as ReceiveUpdates on next tick(s).

## Hubs

| Hub | Path | Purpose |
|-----|------|---------|
| **WorldHub** | /gamehub | JoinWorld(worldId). Server → ReceiveUpdates (deltas) to group; ReceiveTick(tickNumber) every simulation tick. |
| **GameHub** | /commandhub | MoveFleet(sessionId, fleetId, toSystemId, clientTick), QueueBuild, StartRally, StartMartialLaw. → ICommandIngress. |

## REST

| Area | Endpoints |
|------|-----------|
| Session | POST /game/session, POST /game/session/{id}/join, GET /game/sessions, GET /game/session/{id}, GET /game/state |
| Commands | POST /game/move-fleet?worldId=, POST /game/session/{sessionId}/commands/move-fleet (MoveFleet); POST /game/build-structure, /game/attack, /game/diplomacy (CommandQueue) |
| Auth | POST /token |
| Health | GET /, /health, /health/game |
| Events | GET /game/{worldId}/events, POST /game/{worldId}/snapshot |
| Leaderboard | GET /leaderboard/{sessionId}, .../player/{playerId}, /leaderboard/top |

**Create session:** `POST /game/session` body `{ "sessionName": "...", "sessionType": "singleplayer" }` → 201 with sessionId + world. **Move fleet:** `POST /game/session/{sessionId}/commands/move-fleet` body `{ playerId, clientTick, fleetId, toSystemId }` → 202.
