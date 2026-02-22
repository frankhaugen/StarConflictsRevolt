# Done pile

Completed work items. Newest at the top. When you move an item from [current.md](current.md), paste it here and add **Completed:** date (and optional ref).

---

### [SRP] Split Application into single-responsibility projects (Combat, AI, Application)
- **Summary:** Created **StarConflictsRevolt.Server.Combat** (combat resolution only; refs Domain). Created **StarConflictsRevolt.Server.AI** (IAiStrategy, strategies, AiDifficultyService, AiSessionState; refs Domain, Simulation, EventStorage.Abstractions). **StarConflictsRevolt.Server.Application** now only orchestrates gameplay (engine, session aggregates, command ingress, hubs, event broadcast, session/world/content services) and references Combat and AI. WebApi registers types from AI and Combat; solution and architecture doc updated.
- **Completed:** 2025-02-22

### [Cleanup] Move Application layer out of WebApi into Server.Application
- **Summary:** Created **StarConflictsRevolt.Server.Application** and moved all of `WebApi/Application` (Gameplay, AI, Combat services; WorldHub, GameHub, EventBroadcastService; WorldEngine, SessionAggregateManager, etc.) into it. WebApi now only hosts API endpoints, infrastructure, and DI wiring; it references Application. Updated all usings in WebApi and Tests to `StarConflictsRevolt.Server.Application`. Architecture doc updated.
- **Completed:** 2025-02-22

### [Cleanup] Remove duplicate domain and dead code from Server.WebApi
- **Summary:** Removed `StarConflictsRevolt.Server.WebApi/Core` (entire duplicate domain that belonged in Server.Domain). Removed dead `EntityExtensions.cs` and `ModelExtensions.cs` that referenced Core. WebApi now uses **StarConflictsRevolt.Server.Domain** only. Architecture doc updated: single source of truth for domain is Server.Domain.
- **Completed:** 2025-02-22

---

### [Docs] Add narrative story doc
- **Summary:** Create story.md as a single narrative overview (vision, commands vs events, flow, running).
- **Completed:** 2025-02-21

---

### [Docs] Refine docs (README, architecture, domain, api-transport, encounters, aspire)
- **Summary:** Improve doc index, intros, cross-links, Aspire running/build notes.
- **Completed:** 2025-02-21

---

*No further entries yet. Move items here from [current.md](current.md) when done.*
