# Star Conflicts Revolt – Solution Overview

This repository contains all core projects for the Star Conflicts Revolt game, a modern, event-sourced, real-time 4X strategy game inspired by Star Wars: Rebellion.

## Project Structure

- **StarConflictsRevolt.Clients.Blazor**
  - Modern, web-based game client using Blazor Server
  - Responsive, cross-platform web UI that works in any browser
  - Real-time updates via SignalR, testable architecture
  - See [DesignDocs/BlazorClient.md](DesignDocs/BlazorClient.md)

- **StarConflictsRevolt.Clients.Shared**
  - Shared client logic for HTTP/SignalR communication, authentication, and configuration
  - Used by all clients to avoid duplication

- **StarConflictsRevolt.Clients.Models**
  - DTOs for API requests/responses and world state
  - Shared between server and clients for strong typing

- **StarConflictsRevolt.Server.WebApi**
  - Backend API server (ASP.NET Core)
  - Provides REST and SignalR endpoints for game commands, session management, and real-time updates
  - Uses modular Handlers (not Controllers) for endpoint organization
  - Event-sourced with RavenDB for world state and snapshots

- **StarConflictsRevolt.Aspire.AppHost**
  - Orchestrates local/dev environment (RavenDB, Redis, API, EngineWorker)
  - Ensures local/cloud parity and easy development setup

- **StarConflictsRevolt.Aspire.ServiceDefaults**
  - Shared service defaults for Aspire-based orchestration

- **StarConflictsRevolt.Tests**
  - High-level integration and unit tests using TUnit
  - Tests resolve types from DI for realistic scenarios

## Key Architectural Features

- **Event Sourcing**: All world state changes are events, persisted in RavenDB. Snapshots optimize load/replay.
- **Single-writer Simulation**: Each world/session is managed by a single authoritative simulation loop.
- **SignalR**: Real-time updates to clients, with Redis backplane for scaling.
- **DTOs**: Strictly separated in Clients.Models, never returned as 'object'.
- **Handlers-based API**: Endpoints are grouped by concern (Health, Auth, Session, GameAction, Leaderboard, Event).
- **Testable UI**: Client UI/view logic is structured for testability without requiring a renderer.

## Getting Started

1. **Start the AppHost (local dev):**
   ```bash
   dotnet run --project StarConflictsRevolt.Aspire.AppHost
   ```
   This will spin up RavenDB, Redis, API, and simulation services.

2. **Run the Blazor Client:**
   ```bash
   dotnet run --project StarConflictsRevolt.Clients.Blazor
   ```

3. **Run Tests:**
   ```bash
   dotnet test StarConflictsRevolt.Tests
   ```

## Documentation

- See the `DesignDocs/` folder for architecture, design, and API documentation.
- See each project's README for details and usage.

---

*For more details, see [DesignDocs/ARCHITECTURE.md](DesignDocs/ARCHITECTURE.md) and [DesignDocs/DESIGN.md](DesignDocs/DESIGN.md).*
