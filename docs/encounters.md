# Encounter resolution (minimal)

Abstract resolution when fleets meet in a system. No tactical control; outcome is derived from power and optional variance.

## Model

- When two hostile fleets are in the same system (or one arrives and the other is present), an encounter is resolved.
- **Input**: Attacker power, defender power, optional modifiers (system defenses, loyalty, etc.).
- **Output**: Win / Stalemate / Loss. Casualties or state changes applied via events (e.g. EncounterResolved).

## Formula (minimal)

- `result = myPower - enemyPower + randomVariance`
- Thresholds determine Win / Stalemate / Loss. Both sides may survive with reduced strength in a stalemate.

Detailed combat (rounds, initiative, Death Star, etc.) is out of scope for the minimal backend and can be added later.
