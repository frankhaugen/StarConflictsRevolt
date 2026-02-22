# Getting started

Run the game stack, create a session, join it, and send a move-fleet command. Use this after you have built the solution and have Docker available for the AppHost.

---

## Prerequisites

- **.NET SDK** (e.g. .NET 10) — [operations/development.md](operations/development.md) uses the same SDK as the solution.
- **Docker** — Required for Redis, SQL Server, and RavenDB when running the AppHost with containers (default). See [operations/aspire.md](operations/aspire.md) for running without containers.
- **Solution built** — `dotnet build StarConflictsRevolt.slnx`. Stop the AppHost first if it is running, or the build may fail with a locked exe.

---

## 1. Run the stack

From the repository root:

```bash
dotnet run --project StarConflictsRevolt.Aspire.AppHost
```

- The console prints the **Aspire dashboard** URL (e.g. `http://localhost:15xxx`). Open it in a browser.
- In the dashboard you will see **webapi**, **blazor**, and the resource containers (redis, gameDb, ravenDb). Wait until webapi and blazor show as healthy.
- Use the dashboard to open the **Blazor** app and the **webapi** (e.g. for REST or OpenAPI).

---

## 2. Create a session (REST)

Call the webapi (base URL = the webapi endpoint from the dashboard, e.g. `https://localhost:72xxx`):

**Request**

```http
POST /game/session
Content-Type: application/json
Authorization: Bearer <your-token>

{
  "sessionName": "My first game",
  "sessionType": "singleplayer"
}
```

**Response (201)**

```json
{
  "sessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "world": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "galaxy": { "starSystems": [ ... ] }
  }
}
```

Save `sessionId` (and treat it as your **worldId** for commands and hubs). To obtain a token, use `POST /token` with your auth payload (see [reference/api-transport.md](reference/api-transport.md)).

---

## 3. Join the world (SignalR)

To receive live updates (deltas), connect to **WorldHub** and join the world’s group:

1. Connect to **WorldHub** at `{webapiBaseUrl}/gamehub`.
2. Invoke **JoinWorld(worldId)** with the `sessionId` from step 2. The server adds the connection to the group for that world and may send the full world or confirm join.
3. Listen for **ReceiveUpdates** — the server pushes deltas (and event-shaped updates) to the group as the game ticks (e.g. fleet orders accepted, fleet arrived).

Blazor does this via the shared SignalR client; see [reference/api-transport.md](reference/api-transport.md) for hub methods.

---

## 4. Send a move-fleet command

You can send commands via **GameHub** (SignalR) or **REST**. Both go through the same pipeline (Ingress → Queue → Engine).

**REST example (move fleet)**

```http
POST /game/session/{sessionId}/commands/move-fleet
Content-Type: application/json
Authorization: Bearer <your-token>

{
  "playerId": "00000000-0000-0000-0000-000000000001",
  "clientTick": 0,
  "fleetId": "<fleet-guid-from-world>",
  "toSystemId": "<target-star-system-or-planet-guid>"
}
```

**Response:** 202 Accepted (command enqueued; outcome appears as events/deltas on the next tick(s)).

Alternative: `POST /game/move-fleet?worldId={worldId}` with body containing `playerId`, `fleetId`, `fromPlanetId`, `toPlanetId` (see [reference/api-transport.md](reference/api-transport.md)).

---

## 5. What happens on the next tick

1. **GameTickService** publishes a tick (~10 times per second).
2. **WorldEngine** drains the command queue, runs the sim for your MoveFleet, and produces a **FleetOrderAccepted** event (or **CommandRejected** if invalid).
3. The event is applied to the world (fleet goes “in transit” with an **EtaTick**).
4. The server pushes **ReceiveUpdates** (deltas) to the WorldHub group; your client updates the UI.
5. On later ticks, when the current tick reaches the fleet’s **EtaTick**, the engine applies **FleetArrived** and pushes deltas again so the fleet appears at the destination.

So: **send command → next tick processes it → receive deltas → a few ticks later fleet arrives and you get more deltas.**

---

## 6. Play via the Blazor app

1. Open the **Blazor** app from the Aspire dashboard.
2. Log in / obtain a token if required.
3. Create or join a session (the UI will call the REST and hub endpoints above).
4. Use the UI to move fleets; the server ticks and pushes updates so the map stays in sync.

---

## Next steps

- **[reference/api-transport.md](reference/api-transport.md)** — All hub methods and REST endpoints, auth, examples.
- **[reference/architecture.md](reference/architecture.md)** — How the pipeline and tick loop work; event types.
- **[operations/troubleshooting.md](operations/troubleshooting.md)** — Build failures, unhealthy containers, 404s, no deltas.
- **[operations/development.md](operations/development.md)** — Build, test, run options; where code lives.
