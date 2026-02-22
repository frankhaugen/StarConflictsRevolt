# Domain

Minimal Sins/Rebellion-style: **map** (systems, fleets, ETA), **economy**, **abstract encounters**. No tactical combat or deep missions in slice.

## World & session

| Concept | Meaning |
|---------|---------|
| **Session** | One game instance (GUID). Create: POST /game/session. One aggregate, one world. |
| **World** | Full state: Id (= session), Galaxy (star systems, planets), Players, fleets. Event-sourced. |
| **SessionAggregate** | In-memory world + Apply(event). Engine reads → sim → apply → persist → push deltas. |

One session = one world = one aggregate. worldId = sessionId for hubs/REST.

## Map & economy

- **Galaxy** — Star systems (nodes); industry, loyalty, defenses; graph edges.
- **Fleets** — Id, owner, location (planet or in transit), EtaTick. No per-ship list.
- **Economy** — SystemState (Industry, ProductionRate); FactionEconomy (Credits, ShipyardSlots). Industry → credits/capacity; build cost = base + modifier.
- **Loyalty** — Confederation: loyalty affects output; influence to raise. Alliance: martial law → output up, compliance down.

## Fleet movement

- **EtaTick** = arrival tick. **FleetOrderAccepted** → fleet on destination planet, Status=Moving, EtaTick set. When tick ≥ EtaTick, **FleetArrived** → Status=Idle, location set. See [architecture.md](architecture.md) (tick loop).
- **Encounters** — Same system: power + variance → Win/Stalemate/Loss. [encounters.md](encounters.md).

## Out of scope (slice)

Detailed combat, missions, tech trees, victory conditions, deep diplomacy. Minimal loop: **tick → economy → move (ETAs) → resolve encounters → events**.
