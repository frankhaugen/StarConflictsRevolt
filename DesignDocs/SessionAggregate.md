# Session Aggregate & Event Application Architecture

## Overview
Each game session (world) is managed by a **SessionAggregate** that:
- Loads the current world state from a **snapshot** (if available) and **replays events** from RavenDB.
- Applies new events (MoveFleet, BuildStructure, Attack, Diplomacy, etc.) to mutate the world state.
- Produces the new world state and deltas for clients (for SignalR updates).
- Periodically saves a new snapshot to optimize replay.

## Data Flow
1. **Session Start:**
   - On session join or creation, load the latest snapshot from RavenDB (if any).
   - Replay all events after the snapshot to reconstruct the current world state.
2. **Event Handling:**
   - When a new event is received (from HTTP or AI), apply it to the aggregate:
     - Validate the event (e.g., check fleet ownership, valid move, etc.).
     - Mutate the in-memory world state accordingly.
     - Store the event in RavenDB.
     - Broadcast the resulting deltas to clients via SignalR.
3. **Snapshotting:**
   - After every N events (e.g., 500), save a new snapshot of the world state to RavenDB.
   - Delete old events up to the snapshot version to keep the event store lean.

## Aggregate Structure
```csharp
public class SessionAggregate
{
    public Guid SessionId { get; set; }
    public World World { get; set; }
    public int Version { get; set; }
    public List<IGameEvent> UncommittedEvents { get; } = new();

    public void Apply(IGameEvent e) { /* mutate World */ }
    public void LoadFromSnapshot(SessionSnapshot snap) { /* ... */ }
    public void ReplayEvents(IEnumerable<IGameEvent> events) { /* ... */ }
}
```

## Event Application Example
- **MoveFleetEvent:** Find the fleet and update its location from FromPlanetId to ToPlanetId.
- **BuildStructureEvent:** Add the structure to the planet's list of structures.
- **AttackEvent:** Resolve combat, update fleet/planet states.
- **DiplomacyEvent:** Update player relations in the world state.

## Persistence
- **RavenDB** stores:
  - All events for each session (event sourcing)
  - Snapshots of world state (for fast load)
- **SQL Server** (optional):
  - Player accounts, session metadata, leaderboards, etc.

## Integration with Backend
- The backend (GameEngine) maintains a dictionary of active SessionAggregates (by sessionId).
- On event, the aggregate is loaded (or created), the event is applied, and the new state is saved/broadcast.
- On server restart, aggregates are reconstructed from snapshot + events.

## SignalR & Client Updates
- After each event, the aggregate produces a list of deltas (GameObjectUpdate) for SignalR.
- Clients receive only the changes, not the full world state, for efficiency.

## Example Flow
1. Player sends MoveFleet command via HTTP.
2. Backend loads SessionAggregate for sessionId.
3. Applies MoveFleetEvent, mutates world, stores event.
4. Broadcasts delta to clients in session group.
5. After 500 events, saves a new snapshot and prunes old events.

---

This architecture ensures:
- Full replayability and audit trail (event sourcing)
- Efficient load and recovery (snapshots)
- Real-time updates to clients (SignalR)
- Clean separation of concerns (aggregate = single source of truth) 