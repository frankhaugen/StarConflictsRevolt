# Done pile

Completed work items. Newest at the top. When you move an item from [current.md](current.md), paste it here and add **Completed:** date (and optional ref).

---

### [Simulation] ISimulationManager and editable game speed in real time
- **Summary:** Game speed is now controllable in real time. **ISimulationManager** (Server.Simulation.Engine) exposes **TicksPerSecond** (1–120), **GetTickInterval()**, **SetTicksPerSecond(int)**, **IsPaused**, and **SetPaused(bool)**. **SimulationManager** is a thread-safe implementation. **GameTickService** depends on **ISimulationManager**: each loop it uses **GetTickInterval()** for the delay and skips publishing when **IsPaused**. REST API: **GET /game/simulation** returns state (ticksPerSecond, min, max, isPaused); **PATCH /game/simulation** accepts **{ ticksPerSecond?, isPaused? }** and returns updated state. Client: **SimulationStateDto** in Clients.Models; **GetSimulationStateAsync** and **PatchSimulationAsync** in HttpApiClientExtensions (**PatchAsync** added to IHttpApiClient). **GameSpeedControl** component in the Single Player sidebar: slider for ticks/sec and Pause checkbox, updates server in real time.
- **Completed:** 2025-02-22

---

### [UX] Game view as composite components with error handling
- **Summary:** The game view (GameLayout used by SinglePlayer and Galaxy) is now a composite of smaller components so issues can be localized and each section has built-in error handling. **GameViewSection** wraps content in the framework `ErrorBoundary` and shows a compact error UI (section title, message, Retry) when a section fails. **GameLayout** wraps SessionInfo, SidebarContent, QuickActions, and ChildContent in GameViewSection so a failure in one area does not blank the whole view. New components: **GameSessionInfo** (session card, optional resources/turn), **GameSidebarSystems** (list of star systems), **GalaxyMap** (map with coordinates and selection callback), **SelectedSystemCard** (floating selected-system card), **GameMessagesPanel** (messages list with clear). SinglePlayer and Galaxy pages now compose these components instead of inline markup; map and coordinate logic live in GalaxyMap for reuse.
- **Completed:** 2025-02-22

---

### [Gameplay] Building structures end-to-end
- **Summary:** Build-structure flow fixed so the server applies the command with the correct player and world. (1) **Server:** POST `/game/build-structure` now requires `worldId` query (valid session id; uses `TryGetValue` to avoid missing-key exception); request body includes `PlayerId` from client. (2) **Domain:** `BuildStructureEvent` allows building on unowned planets (`OwnerId == null`); when applying, sets `OwnerId = planet.OwnerId ?? PlayerId` so the builder claims the planet. (3) **Client:** `SessionResponse` has `PlayerId`; create-session and join-session set it (human/first or second player). `GameStateService` stores `CurrentPlayerId`, exposes it, and `BuildStructureAsync` sends session id as `worldId` and `CurrentPlayerId` in the body. `HttpApiClientExtensions.BuildStructureAsync` takes `worldId` and `playerId` and sends full payload. (4) **PlanetManager:** Structure dropdown uses domain enum values (Mine, Refinery, ConstructionYard, TrainingFacility, ShieldGenerator, Shipyard); cost display shows Minerals and Energy matching server `GetStructureCosts`. Test mock `MockGameStateService` implements `CurrentPlayerId`.
- **Completed:** 2025-02-22

---

### [Gameplay] AI in the game and Combat Resolution
- **Summary:** (1) **AI part of the game:** Default world now includes a Human and an "AI Commander" player. `WorldFactory` takes optional `IAiStrategy` and adds both to `world.Players` in `CreateDefaultWorld()`, so every new session has an AI opponent and `AiTurnTickListener` processes AI turns each tick. (2) **Combat Resolution:** Attack commands are resolved by the Combat module instead of the simple power-based logic in `AttackEvent`. Added `CombatResolutionService` (Application): when the command is `AttackEvent`, it runs `ICombatSimulator.SimulateFleetCombatAsync(attacker, defender, planet)` and produces an `ApplyCombatResultEvent` with destroyed ship ids and survivor healths. Added `ApplyCombatResultEvent` (Domain) to apply that result (remove destroyed ships, update survivor health, remove empty fleets). `CombatResult` now includes `AttackerSurvivorHealths` and `DefenderSurvivorHealths` (set in `CombatResultCalculator`). `GameUpdateService.ProcessCommandAsync` uses `CombatResolutionService` for attack commands and applies the resulting event.
- **Completed:** 2025-02-22

---

### [Gameplay] Token identity, diagnostics, fleets, galaxy orientation
- **Summary:** (1) **Token and game identity:** Token requests now use the same client id as the game session. Added `AuthClientIdContext` (AsyncLocal) so `CachingTokenProvider` uses the Blazor-stored player id (from `IClientIdProvider` / `getPlayerId()`) when set; `GameStateService` calls `EnsureAuthContextAsync()` before each API call so token and session share identity. (2) **Diagnostics:** Auth status is logged only when it changes (no more "Authentication successful" every 5s). Activity log shows tick events (throttled); SignalR stats show "Last tick" and "Ticks received". Manual Refresh still adds "Diagnostics data refreshed". (3) **Fleet transit view:** Added `FleetDto` and `PlanetDto.Fleets`; server maps domain fleets in `WorldMappingExtensions`. Fleet Manager shows real fleets: "Fleets in transit" (with ETA) and "Docked fleets", and uses fleet ids for move/attack. Attack modal targets other fleets (with location planet); added `AttackAsync(attacker, defender, locationPlanetId)`. (4) **Galaxy orientation:** Galaxy and SinglePlayer map now use +Y = down (top-down map convention) in `Coords()`.
- **Completed:** 2025-02-22

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
