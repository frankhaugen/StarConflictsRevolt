# TODO: Shared Components Extraction

This folder contains components that need to be extracted from the Raylib and Bliss client projects to avoid duplication and ensure consistency.

## Analysis Summary

### Common Dependencies Found:
1. **HTTP Communication** - Both clients use the same HTTP API endpoints
2. **SignalR Communication** - Both clients need real-time updates via SignalR
3. **Authentication** - Both clients need token-based authentication
4. **Configuration** - Both clients share similar configuration patterns
5. **Client Identity** - Both clients need unique client identification

### Client-Specific Dependencies (NOT to be shared):
- **Rendering** - Raylib views, Bliss rendering, UI components
- **Input Handling** - Client-specific input systems
- **Game State Management** - Client-specific state stores
- **Window Management** - Client-specific window configurations

## Folder Structure

```
TODO/
├── README.md (this file)
└── Shared/
    ├── Communication/
    │   ├── GameApiExtensions.cs (extracted from Raylib)
    │   └── SignalRService.cs (extracted from Raylib)
    ├── Authentication/
    │   ├── IClientIdentityService.cs (extracted from Raylib)
    │   └── ClientIdentityService.cs (extracted from Raylib)
    └── Configuration/
        ├── IClientInitializer.cs (extracted from Raylib)
        └── ClientInitializer.cs (extracted from Raylib)
```

## Tasks to Complete

### 1. Extract Game API Extensions
- [ ] Move `HttpApiClientExtensions.cs` from Raylib to `Shared/Communication/`
- [ ] Remove Raylib-specific dependencies
- [ ] Make it generic for both clients
- [ ] Update both clients to use the shared version

### 2. Extract SignalR Service
- [ ] Move `SignalRService.cs` from Raylib to `Shared/Communication/`
- [ ] Remove Raylib-specific world store dependency
- [ ] Make it use a generic interface for world updates
- [ ] Update both clients to use the shared version

### 3. Extract Client Identity Service
- [ ] Move authentication services from Raylib to `Shared/Authentication/`
- [ ] Remove Raylib-specific user profile dependency
- [ ] Make it generic for both clients
- [ ] Update both clients to use the shared version

### 4. Extract Client Initializer
- [ ] Move configuration services from Raylib to `Shared/Configuration/`
- [ ] Remove Raylib-specific render context dependency
- [ ] Make it generic for both clients
- [ ] Update both clients to use the shared version

### 5. Update Project References
- [ ] Update both client projects to reference the shared components
- [ ] Remove duplicate code from both clients
- [ ] Ensure proper dependency injection setup

## Implementation Notes

### SignalR Service Changes Needed:
- Replace `IClientWorldStore` dependency with a generic `IWorldUpdateHandler`
- Make the service accept a callback for world updates instead of directly updating a store

### Client Identity Service Changes Needed:
- Replace `UserProfile` dependency with a generic `IUserProfileProvider`
- Make the service platform-agnostic

### Client Initializer Changes Needed:
- Replace `RenderContext` dependency with a generic `IClientContext`
- Make the service accept callbacks for setting up client state

## Benefits of This Extraction

1. **DRY Principle** - Eliminate code duplication between clients
2. **Consistency** - Ensure both clients behave identically for shared functionality
3. **Maintainability** - Single place to update shared logic
4. **Testing** - Shared components can be tested once
5. **Future Clients** - Easy to add new clients using the same shared components 