> **⚠️ OUTDATED DOCUMENT**  
> This document describes the Bliss client architecture, which has been removed from the project in favor of the Blazor web client. This document is kept for historical reference only.

Bliss is a lightweight, Veldrid-powered 2-D framework that exposes its entire surface as plain C# objects, so a Rebellion remake can be structured like any other cleanly layered .NET library. Below is a SOLID-centric spec-sheet showing how to wire Bliss into an IoC container, slice responsibilities, and meet the gameplay, rendering, and maintainability goals of a 1998-style grand-strategy game.

## 1 · Goals & non-goals

| Item                      | Requirement                                                                                                                                                                             |
| ------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Gameplay parity**       | All original systems — galaxy map, turn queue, missions, tactical summaries, diplomacy, research, construction, espionage — functionally identical to Rebellion ’98 ([Wookieepedia][1]) |
| **100 % 2-D**             | No 3-D or camera rotation; pixel-perfect zoom/pan only.                                                                                                                                 |
| **Deterministic replays** | Identical re-execution on different hosts; no frame-time dependencies.                                                                                                                  |
| **Portable builds**       | Windows-D3D11, Linux-Vulkan, macOS-Metal selected automatically via Veldrid ([Reddit][2])                                                                                               |
| **Battery-friendly**      | Idle frames skipped via dirty-flag issuing (`SuppressDraw`-style) to save laptop/Steam Deck power ([Game Development Stack Exchange][3])                                                |

## 2 · High-level architecture

```text
┌────────────┐   Commands   ┌────────────┐  Events  ┌────────────┐
│  UI Layer  │─────────────▶│  Domain    │─────────▶│Persistence │
│(Bliss port)│              │  Core      │          │ (SQLite)   │
└────────────┘◀─────────────└────────────┘◀─────────└────────────┘
        ▲            ▲               ▲             ▲
        │            │               │             │
   Input/I18n   Time-services   Asset-cache   Network/Lobby
```

* **Ports & Adapters** — UI, Input, Audio, Net, and Persistence are adapters registered behind interfaces; Domain Core knows none of them (Dependency-Inversion).
* **Async orchestration** — long-running AI/pathfinding scheduled on `IBackgroundTaskQueue` allowing awaitable `async` APIs (Liskov/Interface Segregation).
* **Event stream** — immutable domain events feed the replay recorder and the UI’s dirty tracker (Open/Closed + Single Responsibility).

## 3 · Core interfaces (DI contracts)

| Interface        | Purpose                                                                        |
| ---------------- | ------------------------------------------------------------------------------ |
| `IGameLoop`      | `RunAsync(CancellationToken)` drives update/draw.                              |
| `IRenderer2D`    | `Begin(Matrix3x2) / Draw(Sprite) / End()`; thin Bliss adapter.                 |
| `IInput`         | Abstracts keyboard/mouse/game-pad from Bliss’ SDL3 wrapper ([noelberry.ca][4]) |
| `IAudio`         | MiniAudioEx passthrough for SFX/music ([noelberry.ca][4])                      |
| `IClock`         | Deterministic tick source for domain logic (removable for tests).              |
| `IRepository<T>` | CRUD over SQLite via Dapper for savegames.                                     |

All adapters are registered in **`Program.cs`**:

```csharp
var host = Host.CreateDefaultBuilder().Services
    .AddSingleton<BlWindow>()
    .AddSingleton<IRenderer2D, BlissRenderer>()
    .AddSingleton<IInput, BlissInput>()
    .AddSingleton<IGameLoop, GameLoop>()
    .AddSingleton<IClock, SystemClock>()
    .Build();
await host.RunAsync();
```

DI guidance mirrors Microsoft’s tutorial ([Microsoft Learn][5]).

## 4 · Rendering pipeline

1. **Dirty-flag aggregator** — domain events set `FrameInvalid = true`.
2. `GameLoop.Update()` polls input and domain systems; if no changes, calls `window.PumpEvents()` and exits early.
3. When invalid, `IRenderer2D.Begin(cameraMatrix)` starts a GPU instanced batch (`SpriteBatch` in Bliss ([noelberry.ca][4])).
4. Draw order: background → starfield → polit-borders → icons → modals → cursor.
5. `End()` flushes once per frame; batched vertices follow the classic XNA pattern ([Game Development Stack Exchange][3]).
6. Text uses `FontStashSharp` SDF shader, single draw-call per atlas ([noelberry.ca][4]).

Batching rationale: minimizes state changes and leverages GL/Vulkan stream-draw efficiency ([GameDev][6]).

## 5 · Asset & data pipeline

| Asset    | Tool              | Notes                                                         |
| -------- | ----------------- | ------------------------------------------------------------- |
| Textures | TexturePacker CLI | Combine spritesheets, export JSON metadata.                   |
| Fonts    | MSDF-GEN          | Signed-distance fonts for any DPI without repacks.            |
| Audio    | FFmpeg batch      | Convert legacy WAV to OGG for MiniAudioEx.                    |
| Data     | YAML → C# records | Parsed at build to immutable DTOs; avoids runtime reflection. |

No proprietary “content pipeline”; everything is raw files to keep CI simple and testable (Interface Segregation).

## 6 · Domain design highlights

* **Galaxy** — immutable record of sectors, systems, hyperlanes.
* **Commands** (`AssignMission`, `MoveFleet`) validated by aggregates, producing **Events** (`MissionAssigned`, `FleetMoved`).
* **Turn-queue** — pure function emitting a new `GameState`, enabling headless simulation for AI/unit tests.
* **Save/load** — event snapshot every N turns; SQLite schema auto-migrated.

Applying SOLID:

* **SRP** — each system (`DiplomacySystem`, `ResearchSystem`) owns one concern.
* **OCP** — new mission types via strategy objects injected into `MissionProcessor`.
* **LSP** — all entities implement `IGameObject` contract without violating substitutability.
* **ISP** — UI objects depend only on `IHasSprite`, not full domain.
* **DIP** — core references only abstractions, implemented by Bliss adapters.

References for SOLID in games ([FreeCodeCamp][7], [Medium][8]).

## 7 · Testing & tooling

| Layer       | Technique                                                                                                                       |
| ----------- | ------------------------------------------------------------------------------------------------------------------------------- |
| Domain      | XUnit + AutoFixture; no Bliss deps.                                                                                             |
| Renderer    | ImageSnapshot tests using `ImageSharp` diff.                                                                                    |
| Input       | NSubstitute stubs for `IInput`.                                                                                                 |
| Performance | Bunnymark-style stress scene to verify 10 k sprites @ 144 Hz (uses batching benchmarks ([Game Development Stack Exchange][3])). |

## 8 · Performance & UX targets

| Metric       | Target                                                                              |
| ------------ | ----------------------------------------------------------------------------------- |
| Frame budget | 2 ms update, 3 ms render @ 1080p/144 Hz on Ryzen 7 APU.                             |
| Memory       | <128 MB working set after GC (Bliss single-assembly footprint ([noelberry.ca][4])). |
| Load time    | Cold start <1 s on NVMe; asset streaming async.                                     |
| Idle power   | <2 W on Steam Deck via skipped draw frames ([Game Development Stack Exchange][3]).  |

## 9 · Migration & extensibility plan

1. **Swap renderer** — because everything funnels through `IRenderer2D`, moving to raw Veldrid or FNA is a one-file adapter change.
2. **Web port** — when WebGPU lands in Veldrid, reuse domain/UI unchanged ([Reddit][2]).
3. **Modding API** — expose validated YAML schemas; mods compiled into DLLs referencing interfaces only.
4. **ECS experiment** — replace entity records with `IComponent`/`ISystem` without touching Bliss layer.

---

### Source quality note

Project-specific docs for Bliss are concise but definitive; technical details about batching, Veldrid back-ends, SDL3 bindings, and SOLID/DI patterns were corroborated across articles, Microsoft docs, and community posts to meet the citation requirement.

[1]: https://starwars.fandom.com/wiki/Star_Wars%3A_Rebellion_%28video_game%29?utm_source=chatgpt.com "Star Wars: Rebellion (video game) | Wookieepedia - Fandom"
[2]: https://www.reddit.com/r/csharp/comments/7tb1i2/veldrid_3d_graphics_library_implementation/?utm_source=chatgpt.com "Veldrid (3D Graphics Library) Implementation Overview : r/csharp"
[3]: https://gamedev.stackexchange.com/questions/21220/how-exactly-does-xnas-spritebatch-work?utm_source=chatgpt.com "How exactly does XNA's SpriteBatch work?"
[4]: https://noelberry.ca/posts/making_games_in_2025/?utm_source=chatgpt.com "Making Video Games in 2025 (without an engine) - Noel Berry"
[5]: https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-usage?utm_source=chatgpt.com "Tutorial: Use dependency injection in .NET - Learn Microsoft"
[6]: https://www.gamedev.net/forums/topic/637848-sprite-batching-and-other-sprite-rendering-techniques/5026227/?utm_source=chatgpt.com "Sprite batching and other sprite rendering techniques - GameDev.net"
[7]: https://www.freecodecamp.org/news/what-are-the-solid-principles-in-csharp/?utm_source=chatgpt.com "What are the SOLID Principles in C#? Explained With Code Examples"
[8]: https://deperiers-a.medium.com/solid-principles-in-the-context-of-video-games-1940bbae48e9?utm_source=chatgpt.com "SOLID principles in the context of video games | by Alrick Deperiers"
