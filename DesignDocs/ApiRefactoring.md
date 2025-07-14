# API Endpoint Handler Refactoring

## Overview

The `MinimalApiHelper` was becoming unwieldy with all endpoints mixed together in a single file. This refactoring introduces a modular API endpoint handler system that organizes endpoints by concern.

## New Structure

### Main Handler
- **`ApiEndpointHandler`** - Orchestrates all sub-handlers and maps all endpoints

### Sub-Handlers by Concern

1. **`HealthEndpointHandler`** - Health checks and status endpoints
   - `GET /` - Welcome message
   - `GET /health` - Health status
   - `GET /health/game` - Game status

2. **`AuthEndpointHandler`** - Authentication and token endpoints
   - `POST /token` - JWT token generation

3. **`SessionEndpointHandler`** - Session management endpoints
   - `GET /game/state` - Get current world state
   - `POST /game/session` - Create new session
   - `POST /game/session/{sessionId}/join` - Join existing session
   - `GET /game/sessions` - List all sessions
   - `GET /game/session/{sessionId}` - Get specific session

4. **`GameActionEndpointHandler`** - Game action endpoints
   - `POST /game/move-fleet` - Move fleet between planets
   - `POST /game/build-structure` - Build structure on planet
   - `POST /game/attack` - Attack another fleet
   - `POST /game/diplomacy` - Diplomatic actions

5. **`LeaderboardEndpointHandler`** - Leaderboard endpoints
   - `GET /leaderboard/{sessionId}` - Get session leaderboard
   - `GET /leaderboard/{sessionId}/player/{playerId}` - Get player stats
   - `GET /leaderboard/top` - Get top players

6. **`EventEndpointHandler`** - Event store and snapshot endpoints
   - `GET /game/{worldId}/events` - Get events for world
   - `POST /game/{worldId}/snapshot` - Create world snapshot

## Benefits

### Organization
- **Separation of Concerns**: Each handler focuses on a specific domain
- **Maintainability**: Easier to find and modify specific endpoint types
- **Readability**: Clear structure makes the API easier to understand

### Scalability
- **Modular Growth**: New endpoint types can be added as new handlers
- **Team Development**: Different developers can work on different handlers
- **Testing**: Each handler can be tested independently

### Code Quality
- **Reduced Complexity**: Smaller, focused files instead of one large file
- **Better Navigation**: IDE can better navigate the codebase
- **Easier Debugging**: Issues are isolated to specific handlers

## Migration

### What Changed
- `MinimalApiHelper.MapMinimalApis()` → `ApiEndpointHandler.MapAllEndpoints()`
- All endpoints moved to appropriate sub-handlers
- No functional changes to endpoints themselves

### What Stayed the Same
- All endpoint URLs remain unchanged
- All endpoint behavior remains identical
- All existing tests continue to pass
- Client code requires no changes

## File Structure

```
StarConflictsRevolt.Server.WebApi/
└── Infrastructure/
    └── Api/
        ├── ApiEndpointHandler.cs          # Main orchestrator
        ├── HealthEndpointHandler.cs       # Health endpoints
        ├── AuthEndpointHandler.cs         # Authentication endpoints
        ├── SessionEndpointHandler.cs      # Session management
        ├── GameActionEndpointHandler.cs   # Game actions
        ├── LeaderboardEndpointHandler.cs  # Leaderboard
        └── EventEndpointHandler.cs        # Event store
```

## Usage

### In StartupHelper.cs
```csharp
// Old way
MinimalApiHelper.MapMinimalApis(app);

// New way
ApiEndpointHandler.MapAllEndpoints(app);
```

### Adding New Endpoints
1. Identify the appropriate handler for your endpoint type
2. Add the endpoint to that handler's `MapEndpoints` method
3. Follow the existing patterns for error handling and authorization

### Adding New Handler Types
1. Create a new handler class following the existing pattern
2. Add a `MapEndpoints(WebApplication app)` method
3. Register it in `ApiEndpointHandler.MapAllEndpoints()`

## Testing

All existing integration tests continue to pass, confirming that:
- No functional changes were introduced
- All endpoints work exactly as before
- The refactoring is purely organizational

## Future Enhancements

This modular structure enables several future improvements:

1. **Handler-Specific Middleware**: Each handler could have its own middleware
2. **Handler-Specific Configuration**: Configuration could be scoped to handlers
3. **Handler-Specific Logging**: Logging could be tailored per handler
4. **Handler-Specific Metrics**: Metrics could be collected per handler
5. **Handler-Specific Documentation**: OpenAPI documentation could be organized by handler

## Conclusion

This refactoring successfully addresses the "uncontrollable" nature of the original `MinimalApiHelper` by introducing a clean, modular structure that maintains all existing functionality while providing a solid foundation for future development. 