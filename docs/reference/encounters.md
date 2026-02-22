# Encounters

Abstract resolution when hostile fleets meet in a system. No tactical control; outcome from **power + optional variance**. [domain.md](domain.md) for context.

- **Input:** Attacker power, defender power, modifiers (defenses, loyalty).
- **Output:** Win / Stalemate / Loss; events (e.g. EncounterResolved) apply casualties/state.
- **Formula (minimal):** `result = myPower - enemyPower + randomVariance`; thresholds → outcome. Stalemate can leave both reduced.

Out of scope: rounds, initiative, Death Star runs.
