# Star Conflicts Revolt - Shared Client Components

This project contains shared components used by all client implementations (Bliss, Blazor, and future clients) to avoid code duplication and ensure consistency.

## Overview

The shared components provide common functionality for:
- **HTTP Communication** - API client extensions for game server communication
- **SignalR Communication** - Real-time updates and session management
- **Authentication** - Client identity and user profile management
- **Configuration** - Client initialization and configuration validation

## Architecture

### Communication Layer
- `HttpApiClientExtensions` - HTTP API client with game-specific methods
- `ISignalRService` / `SignalRService` - Real-time communication with game server

### Authentication Layer
- `IClientIdentityService` / `ClientIdentityService` - Client identity management
- `IUserProfileProvider` - Platform-agnostic user profile access

### Configuration Layer
- `IClientInitializer` / `ClientInitializer` - Client initialization and setup
- `ServiceCollectionExtensions` - Dependency injection configuration

### Infrastructure
- `IClientWorldStore` / `ClientWorldStore` - Game state management
- `IHttpApiClient` - HTTP client abstraction

## Usage

All client projects should reference this shared project and use the provided services:

```csharp
// In client Program.cs or Startup.cs
services.AddSharedClientServices(configuration);
```

## Benefits

1. **DRY Principle** - Eliminates code duplication between clients
2. **Consistency** - Ensures all clients behave identically for shared functionality
3. **Maintainability** - Single place to update shared logic
4. **Testing** - Shared components can be tested once
5. **Future Clients** - Easy to add new clients using the same shared components 