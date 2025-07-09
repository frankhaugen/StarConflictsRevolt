# Star Conflicts Revolt - Raylib Client

A modern, cross-platform game client built with Raylib for the Star Conflicts Revolt game.

## Features

### ✅ Implemented Features

1. **Complete Game State Management**
   - Session management (create/join sessions)
   - Player authentication and identification
   - Game speed control (Paused, Slow, Normal, Fast)
   - Navigation history and view management
   - Feedback system with auto-expiring messages

2. **Modern UI System**
   - Consistent color scheme and styling
   - Interactive buttons, text inputs, and panels
   - Status bars and information panels
   - Confirmation dialogs
   - Minimap visualization

3. **Multiple Game Views**
   - **Menu View**: Session creation, joining, and navigation
   - **Galaxy View**: Interactive star system and planet visualization
   - **Fleet Finder**: Search and manage fleets (with placeholder data)
   - **Planetary Finder**: Browse and select planets
   - **Game Options**: Settings and configuration
   - **Tactical Battle**: Battle visualization and controls

4. **Command System**
   - HTTP-based command sending to game server
   - Proper error handling and feedback
   - Support for all major game actions (move, build, attack, diplomacy)

5. **Real-time Updates**
   - SignalR integration for live game updates
   - Automatic world state synchronization

### 🎮 Controls

#### Navigation
- **F1**: Return to Menu
- **F2**: Fleet Finder
- **F3**: Game Options
- **F4**: Planetary Finder
- **ESC**: Back/Exit

#### Game Options
- **S**: Toggle Sound
- **F**: Toggle Fullscreen
- **G**: Cycle Game Speed

#### Galaxy View
- **Mouse Click**: Select systems/planets
- **M**: Move Fleet (placeholder)
- **B**: Build Structure (placeholder)
- **A**: Attack (placeholder)
- **D**: Diplomacy (placeholder)

## Prerequisites

- .NET 9.0 SDK
- Raylib (included via NuGet package)

## Running the Client

1. **Start the Game Server**
   ```bash
   # From the root directory
   dotnet run --project StarConflictsRevolt.Server.WebApi
   ```

2. **Run the Raylib Client**
   ```bash
   # From the root directory
   dotnet run --project StarConflictsRevolt.Clients.Raylib
   ```

## Configuration

The client configuration is in `appsettings.json`:

```json
{
  "GameClientConfiguration": {
    "GameServerHubUrl": "http://localhost:5267/gamehub"
  }
}
```

## Architecture

### Key Components

- **GameState**: Central state management for session, player, and UI state
- **GameCommandService**: HTTP client for sending game commands
- **UIHelper**: Reusable UI components and styling
- **RenderContext**: Integration layer between UI and game data
- **SignalRService**: Real-time communication with game server

### View System

Each game view implements the `IView` interface:
- `ViewType`: Identifies the view type
- `Draw()`: Renders the view content

Views are registered in `Program.cs` and automatically selected based on the current `GameState.CurrentView`.

## Development Status

### ✅ Complete
- Basic UI framework and styling
- Session management
- Multiple game views
- Command system integration
- Real-time updates

### 🚧 In Progress
- Advanced battle mechanics
- Fleet management details
- Structure building dialogs
- Diplomacy system

### 📋 Planned
- Enhanced graphics and animations
- Sound effects and music
- Multiplayer lobby system
- Save/load functionality
- Advanced AI integration

## Troubleshooting

### Common Issues

1. **"No world data available"**
   - Ensure the game server is running
   - Check that you've joined a session

2. **Connection errors**
   - Verify the server URL in `appsettings.json`
   - Check that the server is accessible on the configured port

3. **Build errors**
   - Ensure .NET 9.0 SDK is installed
   - Run `dotnet restore` to restore packages

## Contributing

When adding new features:

1. Follow the existing UI patterns in `UIHelper`
2. Use the `GameState` for state management
3. Implement proper error handling and feedback
4. Add keyboard shortcuts for common actions
5. Update this README with new features

## License

See the main project LICENSE file. 