using StarConflictsRevolt.Server.WebApi.Enums;
using StarConflictsRevolt.Server.WebApi.Eventing;
using StarConflictsRevolt.Server.WebApi.Models;
using StarConflictsRevolt.Server.WebApi.Services;

namespace StarConflictsRevolt.Server.WebApi.Infrastructure.Api;

/// <summary>
/// Handles game action endpoints (move fleet, build structure, attack, diplomacy)
/// </summary>
public static class GameActionEndpointHandler
{
    public static void MapEndpoints(WebApplication app)
    {
        app.MapPost("/game/move-fleet", async context =>
        {
            var commandQueue = context.RequestServices.GetRequiredService<CommandQueue<IGameEvent>>();
            var sessionManagerService = context.RequestServices.GetRequiredService<SessionAggregateManager>();
            var worldService = context.RequestServices.GetRequiredService<WorldService>();
            var dto = await context.Request.ReadFromJsonAsync<MoveFleetEvent>(context.RequestAborted);
            if (dto == null)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid MoveFleetEvent");
                return;
            }

            var worldId = context.Request.Query.ContainsKey("worldId") ? Guid.Parse(context.Request.Query["worldId"]) : Guid.Empty;
            if (!await sessionManagerService.SessionExistsAsync(worldId))
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync($"Session/world {worldId} does not exist");
                return;
            }

            var world = await worldService.GetWorldAsync(context.RequestAborted);
            // Strict validation
            var fleet = world.Galaxy.StarSystems.SelectMany(s => s.Planets).SelectMany(p => p.Fleets).FirstOrDefault(f => f.Id == dto.FleetId);
            var fromPlanet = world.Galaxy.StarSystems.SelectMany(s => s.Planets).FirstOrDefault(p => p.Id == dto.FromPlanetId);
            var toPlanet = world.Galaxy.StarSystems.SelectMany(s => s.Planets).FirstOrDefault(p => p.Id == dto.ToPlanetId);
            if (fleet == null)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync($"Fleet {dto.FleetId} does not exist");
                return;
            }

            if (fromPlanet == null)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync($"FromPlanet {dto.FromPlanetId} does not exist");
                return;
            }

            if (toPlanet == null)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync($"ToPlanet {dto.ToPlanetId} does not exist");
                return;
            }

            if (!fromPlanet.Fleets.Any(f => f.Id == dto.FleetId))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync($"Fleet {dto.FleetId} is not at FromPlanet {dto.FromPlanetId}");
                return;
            }

            if (fleet.LocationPlanetId == dto.ToPlanetId)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync($"Fleet {dto.FleetId} is already at ToPlanet {dto.ToPlanetId}");
                return;
            }

            // TODO: Check player ownership if available
            commandQueue.Enqueue(worldId, dto);
            context.Response.StatusCode = 202;
        }).RequireAuthorization();

        app.MapPost("/game/build-structure", async context =>
        {
            var commandQueue = context.RequestServices.GetRequiredService<CommandQueue<IGameEvent>>();
            var worldService = context.RequestServices.GetRequiredService<WorldService>();
            var dto = await context.Request.ReadFromJsonAsync<BuildStructureEvent>(context.RequestAborted);
            if (dto == null)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid BuildStructureEvent");
                return;
            }

            var worldId = context.Request.Query.ContainsKey("worldId") ? Guid.Parse(context.Request.Query["worldId"]) : Guid.Empty;
            var world = await worldService.GetWorldAsync(worldId, context.RequestAborted);
            var planet = world.Galaxy.StarSystems.SelectMany(s => s.Planets).FirstOrDefault(p => p.Id == dto.PlanetId);
            if (planet == null)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync($"Planet {dto.PlanetId} does not exist");
                return;
            }

            // Validate structure type
            if (!Enum.TryParse<StructureVariant>(dto.StructureType, out _))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync($"StructureType {dto.StructureType} is not valid");
                return;
            }

            // TODO: Check player permissions/ownership if available
            commandQueue.Enqueue(worldId, dto);
            context.Response.StatusCode = 202;
        }).RequireAuthorization();

        app.MapPost("/game/attack", async context =>
        {
            var commandQueue = context.RequestServices.GetRequiredService<CommandQueue<IGameEvent>>();
            var worldService = context.RequestServices.GetRequiredService<WorldService>();
            var dto = await context.Request.ReadFromJsonAsync<AttackEvent>(context.RequestAborted);
            if (dto == null)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid AttackEvent");
                return;
            }

            var worldId = context.Request.Query.ContainsKey("worldId") ? Guid.Parse(context.Request.Query["worldId"]) : Guid.Empty;
            var world = await worldService.GetWorldAsync(context.RequestAborted);
            var planet = world.Galaxy.StarSystems.SelectMany(s => s.Planets).FirstOrDefault(p => p.Id == dto.LocationPlanetId);
            if (planet == null)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync($"LocationPlanet {dto.LocationPlanetId} does not exist");
                return;
            }

            var attacker = planet.Fleets.FirstOrDefault(f => f.Id == dto.AttackerFleetId);
            var defender = planet.Fleets.FirstOrDefault(f => f.Id == dto.DefenderFleetId);
            if (attacker == null)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync($"AttackerFleet {dto.AttackerFleetId} does not exist at planet {dto.LocationPlanetId}");
                return;
            }

            if (defender == null)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync($"DefenderFleet {dto.DefenderFleetId} does not exist at planet {dto.LocationPlanetId}");
                return;
            }

            if (dto.AttackerFleetId == dto.DefenderFleetId)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Attacker and defender fleets cannot be the same");
                return;
            }

            // TODO: Check player ownership if available
            commandQueue.Enqueue(worldId, dto);
            context.Response.StatusCode = 202;
        }).RequireAuthorization();

        app.MapPost("/game/diplomacy", async context =>
        {
            var commandQueue = context.RequestServices.GetRequiredService<CommandQueue<IGameEvent>>();
            var worldService = context.RequestServices.GetRequiredService<WorldService>();
            var dto = await context.Request.ReadFromJsonAsync<DiplomacyEvent>(context.RequestAborted);
            if (dto == null)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid DiplomacyEvent");
                return;
            }

            var worldId = context.Request.Query.ContainsKey("worldId") ? Guid.Parse(context.Request.Query["worldId"]) : Guid.Empty;
            var world = await worldService.GetWorldAsync(context.RequestAborted);
            // For demo, assume players are fleets' owners (stub)
            var allPlayerIds = world.Galaxy.StarSystems.SelectMany(s => s.Planets).SelectMany(p => p.Fleets).Select(f => f.Id).ToHashSet();
            if (!allPlayerIds.Contains(dto.PlayerId))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync($"PlayerId {dto.PlayerId} does not exist");
                return;
            }

            if (!allPlayerIds.Contains(dto.TargetPlayerId))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync($"TargetPlayerId {dto.TargetPlayerId} does not exist");
                return;
            }

            commandQueue.Enqueue(worldId, dto);
            context.Response.StatusCode = 202;
        }).RequireAuthorization();
    }
} 