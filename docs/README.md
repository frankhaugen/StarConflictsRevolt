# StarConflictsRevolt — documentation

Primary specs for the client-agnostic game backend (Sins/Rebellion-style strategy).

## Current docs

| Document | Description |
|----------|-------------|
| [architecture.md](architecture.md) | Command vs Event, pipeline (Ingress → Queue → Engine), SignalR, event store, single-process topology |
| [domain.md](domain.md) | Minimal map (systems, fleets, ETA), economy, loyalty, what is out of scope |
| [api-transport.md](api-transport.md) | GameHub and WorldHub methods, REST session and command endpoints |
| [encounters.md](encounters.md) | Abstract encounter resolution (no tactical combat) |

## Legacy design docs

The **DesignDocs/** folder at solution root contains older, sometimes over-scoped specs (e.g. MissionSystem, CombatSystem, RebellionGameViews). They are kept for reference but are superseded by the above for the minimal backend. BlissClient.md is outdated (Bliss client removed).
