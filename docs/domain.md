# Domain — Minimal Sins/Rebellion-style strategy

Lean design inspired by Sins of a Solar Empire and Star Wars Rebellion (1998): strategic layer, simple economy, abstract encounters. No deep characters, missions, or tactical combat.

## Map

- **Systems** (nodes): Have Industry, Loyalty, optional Defenses. Connected by a graph (edges).
- **Galaxy**: Collection of star systems. One canonical representation (World.Galaxy with StarSystems).
- **Fleets**: Identity, owner, location (system or in-transit), power rating, ETA tick. No per-ship list in the minimal slice.

## Economy

- **SystemState**: Industry, ProductionRate, Controlled (minimal record for economy).
- **FactionEconomy**: Credits, ShipyardSlots.
- Industry produces credits and shipyard capacity over time. Fleet build cost = base + size modifier.

## Fleet movement and encounters

- Movement is abstracted: fleets have an ETA along graph edges.
- When fleets meet enemy forces in a system: compare power, apply optional variance, resolve as Win / Stalemate / Loss. No RTS micro; "football manager" style resolution only.

## Loyalty / order

- **Confederation**: Systems have Loyalty; lower loyalty reduces output; spend influence to raise.
- **Alliance**: Martial Law temporarily raises output but lowers Compliance (abstract loyalty).

Simple numeric modifiers only; no complex event chains.

## Out of scope (minimal slice)

- Detailed combat (Death Star, trench runs, etc.)
- Missions and character-based operations
- Technology trees
- Victory conditions beyond “hold space”
- Nested political/diplomacy event chains

These can be added later; the minimal loop is: tick → economy → move fleets (ETAs) → resolve encounters → emit events.
