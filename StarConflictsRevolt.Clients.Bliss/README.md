# Star Conflicts Revolt - Bliss Client

A modern, Veldrid-powered 2D game client built with the Bliss library for the Star Conflicts Revolt game. This implementation follows SOLID principles and provides a clean, layered architecture for a 1998-style grand-strategy game.

## Features

### ✅ Implemented Features

1. **SOLID Architecture**
   - **Single Responsibility**: Each component has one clear purpose
   - **Open/Closed**: New views can be added without modifying existing code
   - **Liskov Substitution**: All interfaces are properly substitutable
   - **Interface Segregation**: UI objects depend only on necessary abstractions
   - **Dependency Inversion**: Core logic depends on abstractions, not concrete implementations

2. **Battery-Friendly Rendering**
   - Dirty-flag optimization skips unnecessary draw frames
   - Idle frames are suppressed to save laptop/Steam Deck power
   - Efficient sprite batching with GPU instancing

3. **Modern UI System**
   - Consistent view-based architecture
   - Interactive navigation with keyboard and mouse
   - Feedback system with auto-expiring messages
   - Confirmation dialogs

4. **Multiple Game Views**
   - **Menu View**: Main navigation and session management
   - **Galaxy View**: Interactive star system visualization with camera movement
   - **Game Options**: Settings and configuration management

5. **Cross-Platform Support**
   - Windows-D3D11, Linux-Vulkan, macOS-Metal via Veldrid
   - Automatic backend selection
   - Portable builds

### 🎮 Controls

#### Navigation
- **F1**: Return to Menu
- **F2**: Fleet Finder (placeholder)
- **F3**: Game Options
- **F4**: Planetary Finder (placeholder)
- **ESC**: Back/Exit

#### Menu Navigation
- **UP/DOWN**: Navigate menu options
- **ENTER**: Select option

#### Galaxy View
- **WASD**: Move camera
- **Mouse Click**: Select positions
- **ESC**: Return to menu

#### Game Options
- **UP/DOWN**: Navigate options
- **LEFT/RIGHT**: Toggle settings
- **ENTER**: Confirm selection

## Architecture

### High-Level Design

```text
┌────────────┐   Commands   ┌────────────┐  Events  ┌────────────┐
│  UI Layer  │─────────────▶│  Domain    │─────────▶│Persistence │
│(Bliss port)│              │  Core      │          │ (Future)   │
└────────────┘◀─────────────└────────────┘◀─────────└────────────┘
        ▲            ▲               ▲             ▲
        │            │               │             │
   Input/I18n   Time-services   Asset-cache   Network/Lobby
```

### Core Interfaces (DI Contracts)

| Interface        | Purpose                                                                        |
| ---------------- | ------------------------------------------------------------------------------ |
| `IGameLoop`      | `RunAsync(CancellationToken)` drives update/draw cycle                        |
| `IRenderer2D`    | `Begin(Matrix3x2) / Draw(Sprite) / End()`; thin Bliss adapter                 |
| `IInput`         | Abstracts keyboard/mouse/gamepad from Bliss's SDL3 wrapper                    |
| `IClock`         | Deterministic tick source for domain logic (removable for tests)              |
| `IView`          | Game view interface with update/draw lifecycle                                |
| `IViewFactory`   | Factory for creating views based on view type                                 |

### Rendering Pipeline

1. **Dirty-flag aggregator** — domain events set `FrameInvalid = true`
2. `GameLoop.Update()` polls input and domain systems; if no changes, calls `window.PumpEvents()` and exits early
3. When invalid, `IRenderer2D.Begin(cameraMatrix)` starts a GPU instanced batch
4. Draw order: background → starfield → UI elements → cursor
5. `End()` flushes once per frame; batched vertices follow the classic XNA pattern
6. Text uses Bliss's built-in font rendering with SDF shaders

## Prerequisites

- .NET 9.0 SDK
- Bliss library (included via NuGet package)

## Running the Client

1. **Start the Game Server** (if available)
   ```bash
   # From the root directory
   dotnet run --project StarConflictsRevolt.Server.WebApi
   ```

2. **Run the Bliss Client**
   ```bash
   # From the root directory
   dotnet run --project StarConflictsRevolt.Clients.Bliss
   ```

## Configuration

The client configuration is in `appsettings.json`:

```json
{
  "GameClientConfiguration": {
    "ApiUrl": "http://localhost:5267",
    "GameServerHubUrl": "http://localhost:5267/gamehub",
    "WindowWidth": 1280,
    "WindowHeight": 720,
    "WindowTitle": "Star Conflicts Revolt - Bliss Client"
  }
}
```

## Performance Targets

| Metric       | Target                                                                              |
| ------------ | ----------------------------------------------------------------------------------- |
| Frame budget | 2 ms update, 3 ms render @ 1080p/144 Hz on Ryzen 7 APU                             |
| Memory       | <128 MB working set after GC (Bliss single-assembly footprint)                     |
| Load time    | Cold start <1 s on NVMe; asset streaming async                                      |
| Idle power   | <2 W on Steam Deck via skipped draw frames                                         |

## Development Status

### ✅ Complete
- SOLID architecture implementation
- Basic UI framework and view system
- Battery-friendly rendering with dirty-flag optimization
- Cross-platform Bliss integration
- Input abstraction layer
- Game state management

### 🚧 In Progress
- Asset loading and management
- Network integration with game server
- Advanced rendering features

### 📋 Planned
- Session management and multiplayer
- Advanced battle mechanics
- Fleet management details
- Structure building dialogs
- Diplomacy system
- Save/load functionality

## Testing

The architecture supports comprehensive testing:

| Layer       | Technique                                                                           |
| ----------- | ----------------------------------------------------------------------------------- |
| Domain      | XUnit + AutoFixture; no Bliss dependencies                                         |
| Renderer    | ImageSnapshot tests using image diff                                               |
| Input       | NSubstitute stubs for `IInput`                                                     |
| Performance | Bunnymark-style stress scene to verify 10k sprites @ 144 Hz                        |

## Migration & Extensibility

1. **Swap renderer** — because everything funnels through `IRenderer2D`, moving to raw Veldrid or FNA is a one-file adapter change
2. **Web port** — when WebGPU lands in Veldrid, reuse domain/UI unchanged
3. **Modding API** — expose validated schemas; mods compiled into DLLs referencing interfaces only
4. **ECS experiment** — replace entity records with `IComponent`/`ISystem` without touching Bliss layer

## Troubleshooting

### Common Issues

1. **"Bliss library not found"**
   - Ensure Bliss NuGet package is properly installed
   - Check that .NET 9.0 SDK is installed

2. **"Window creation failed"**
   - Verify graphics drivers are up to date
   - Check that Veldrid backends are available for your platform

3. **Performance issues**
   - Monitor frame rate with debug logging enabled
   - Check that dirty-flag optimization is working

## Contributing

This implementation follows the SOLID principles and clean architecture patterns. When adding new features:

1. **Follow Interface Segregation** — UI objects should depend only on necessary abstractions
2. **Use Dependency Injection** — Register new services in `ServiceCollectionExtensions`
3. **Implement Dirty-Flag Optimization** — Invalidate frames only when necessary
4. **Add Tests** — Ensure new components are testable without Bliss dependencies

## License

This project is part of the Star Conflicts Revolt game and follows the same licensing terms. 