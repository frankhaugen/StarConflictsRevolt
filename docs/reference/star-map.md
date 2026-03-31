# 2D Star Map (Real Galaxy, Discified)

The game can display a **2D star map** built from real galactic star data. The data is "discified": 3D positions are projected onto a 2D disc and optionally thinned so the map stays usable.

## Data source

- **Model:** `StarConflictsRevolt.Server.WebApi.StarSystem` (and `Star`, `Coordinates`) in the WebApi project.
- **Data:** Real star systems from our galaxy; loaded from embedded **starsystems.json** (or optionally from file).
- **Coordinates:** 3D galactic coordinates in parsecs (X, Y, Z; Sol at origin, `DistanceFromSol`).

## Discify process

1. **Project to 2D:** Use the galactic plane (X, Y) as the 2D map; Z is height above/below the plane.
2. **Filter:**
   - **Distance:** Only stars within `MaxDistanceFromSolParsecs` of Sol (default 500 pc).
   - **Thin disc:** Only stars with `|Z| ≤ MaxAbsZParsecs` (default 100 pc).
3. **Subsample:** If the remaining count exceeds `MaxStars` (default 4000), take a regular subsample so the map stays responsive.
4. **Scale:** Map (X, Y) into the range `[-ScaleToRange, +ScaleToRange]` (default 1000) so the Blazor `GalaxyMap` component can place nodes in its 10–90% CSS band.

Options are configurable via `DiscifyOptions` (server) and query parameters on the API.

## API

- **GET /game/starmap**
  - **Query:** `maxDistance` (pc), `maxZ` (pc), `maxStars` (cap).
  - **Response:** JSON array of `StarMapPointDto`: `{ "name": string, "coordinates": { "x": number, "y": number } }`.
  - Coordinates are in the same scale as game star systems (±1000) for direct use in the galaxy map.

## Client usage

- **Galaxy page:** When there is no current game world, the Galaxy view fetches `/game/starmap` and displays the result as read-only star points (no planets). When a session has a world, the normal game star systems are shown instead.
- **StarMapPointDto** is converted to **StarSystemDto** with empty planets and a placeholder Id for rendering in the same `GalaxyMap` component.

## Files

| Area | File |
|------|------|
| Model & loaders | `StarConflictsRevolt.Server.WebApi/StarSystem.cs` |
| Discify options & logic | `StarSystem.cs` (`DiscifyOptions`, `DiscifyAsync`) |
| Service | `StarConflictsRevolt.Server.WebApi/API/Services/StarMapService.cs` |
| Endpoint | `StarConflictsRevolt.Server.WebApi/API/Handlers/Endpoints/StarMapEndpointHandler.cs` |
| DTO (client) | `StarConflictsRevolt.Clients.Models/StarMapPointDto.cs` |
| HTTP extension | `StarConflictsRevolt.Clients.Shared/Http/HttpApiClientExtensions.cs` (`GetStarMapAsync`) |
| UI | `StarConflictsRevolt.Clients.Blazor/Components/Pages/Galaxy.razor` |

## Extending

- **World generation:** The same discified data could be used to seed game worlds (e.g. use real star positions and names, attach generated planets).
- **Caching:** For large `starsystems.json`, consider caching the discified result (e.g. by query params) in `StarMapService`.
- **Embedded vs file:** The helper supports both `LoadStarSystemsFromEmbeddedAsync()` (default) and `LoadStarSystemsFromFileAsync(path)`; the service currently uses the embedded resource.
