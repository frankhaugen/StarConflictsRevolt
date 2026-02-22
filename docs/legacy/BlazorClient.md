# Blazor Client Architecture

## Overview

The Blazor client is the primary web-based UI implementation for Star Conflicts Revolt. It provides a modern, responsive web interface that can run in any browser without requiring installation or platform-specific dependencies.

## 🎯 Goals & Architecture

| Item                      | Requirement                                                                                                                                                                             |
| ------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Cross-platform**        | Runs in any modern web browser on any operating system                                                                                                                                 |
| **Real-time updates**     | Uses SignalR for low-latency real-time game state updates                                                                                                                              |
| **Responsive design**     | Works on desktop, tablet, and mobile devices                                                                                                                                           |
| **Modern UI**             | Clean, sci-fi inspired interface with smooth animations and transitions                                                                                                                |
| **Testable architecture** | UI logic is structured to be testable without requiring the actual renderer                                                                                                            |

## 🏗️ High-level Architecture

```text
┌────────────┐   HTTP/SignalR   ┌────────────┐  Events  ┌────────────┐
│  Blazor UI │─────────────────▶│  GameState │─────────▶│  Server    │
│ Components │                  │  Service   │          │  WebAPI    │
└────────────┘◀─────────────────└────────────┘◀─────────└────────────┘
        ▲              ▲               ▲             ▲
        │              │               │             │
        │              │               │             │
   ┌─────────┐   ┌─────────────┐  ┌─────────┐  ┌─────────────┐
   │ Razor   │   │ SignalR     │  │ DTOs    │  │ RavenDB     │
   │ Pages   │   │ Service     │  │ Models  │  │ Event Store │
   └─────────┘   └─────────────┘  └─────────┘  └─────────────┘
```

## 📁 Project Structure

```
StarConflictsRevolt.Clients.Blazor/
├── Components/
│   ├── Layout/              # Navigation and layout components
│   └── Pages/               # Main game pages (Home, Galaxy, Sessions, etc.)
├── Services/
│   ├── GameStateService.cs  # Main game state management
│   ├── BlazorSignalRService.cs # SignalR wrapper for Blazor
│   └── IGameStateService.cs # Service interface
├── Program.cs               # Application startup and DI configuration
└── appsettings.json        # Client configuration
```

## 🔧 Key Components

### GameStateService
- Manages the current game state and session
- Handles communication with the server via HTTP and SignalR
- Provides events for UI updates when game state changes
- Implements `IGameStateService` interface for testability

### BlazorSignalRService
- Wraps the shared `ISignalRService` for Blazor-specific needs
- Handles real-time updates from the server
- Manages connection state and reconnection logic

### Razor Components
- **Home.razor**: Main menu and game entry point
- **Galaxy.razor**: Galaxy map view with star systems and fleets
- **Sessions.razor**: Session browser and management
- **SinglePlayer.razor**: Single-player game setup

## 🚀 Features

### Real-time Updates
- Uses SignalR for instant game state synchronization
- Automatic reconnection on connection loss
- Delta updates to minimize bandwidth usage

### Responsive Design
- Bootstrap-based responsive layout
- Works on desktop, tablet, and mobile devices
- Touch-friendly controls for mobile users

### Modern UI
- Clean, sci-fi inspired design
- Font Awesome icons for visual elements
- Smooth animations and transitions
- Dark theme optimized for gaming

### Testable Architecture
- All UI logic is separated from rendering concerns
- Services are injected via dependency injection
- Components can be tested without browser rendering

## 🔌 Integration

### Shared Components
- Uses `StarConflictsRevolt.Clients.Shared` for common functionality
- HTTP client configuration and API communication
- SignalR connection management
- Authentication and user management

### Server Communication
- HTTP API calls for session management and game actions
- SignalR for real-time world state updates
- Proper error handling and retry logic

### Configuration
- Environment-specific configuration via `appsettings.json`
- Configurable server endpoints and connection settings
- Development and production environment support

## 🧪 Testing

The Blazor client includes comprehensive integration tests:

- **BlazorClientIntegrationTests**: Tests service resolution and basic functionality
- **BlazorApplicationStartupTests**: Validates application startup and error handling
- **BlazorProgramConfigurationTests**: Tests configuration binding and service lifetimes

All tests use the TUnit framework and follow the project's testing patterns.

## 🚀 Deployment

The Blazor client can be deployed as:

1. **Standalone Web Application**: Self-contained web app with embedded server
2. **Docker Container**: Containerized deployment for cloud platforms
3. **Azure App Service**: Direct deployment to Azure web services
4. **Local Development**: Integrated with Aspire AppHost for local development

## 📱 Browser Support

- Chrome/Chromium 90+
- Firefox 88+
- Safari 14+
- Edge 90+

## 🔮 Future Enhancements

- Progressive Web App (PWA) support for offline capabilities
- WebAssembly (WASM) version for better performance
- Advanced UI components and animations
- Mobile-specific optimizations
- Accessibility improvements
