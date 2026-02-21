using StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Commands;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Enums;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Events;

namespace StarConflictsRevolt.Server.WebApi.API.Handlers.Endpoints;

public record MoveFleetCommandDto(Guid PlayerId, long ClientTick, Guid FleetId, Guid ToSystemId);

/// <summary>
///     Handles game action endpoints. Command endpoints submit via ICommandIngress (same pipeline as GameHub).
/// </summary>
public static class GameActionEndpointHandler
{
    public static void MapEndpoints(WebApplication app)
    {
        app.MapPost("/game/move-fleet", async context =>
        {
            var ingress = context.RequestServices.GetRequiredService<ICommandIngress>();
            var sessionManagerService = context.RequestServices.GetRequiredService<SessionAggregateManager>();
            var dto = await context.Request.ReadFromJsonAsync<MoveFleetEvent>(context.RequestAborted);
            if (dto == null)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid MoveFleetEvent");
                return;
            }

            var worldId = context.Request.Query.TryGetValue("worldId", out var wv) && Guid.TryParse(wv, out var w) ? w : Guid.Empty;
            if (worldId == Guid.Empty || !await sessionManagerService.SessionExistsAsync(worldId))
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("Session/world not found or missing worldId query");
                return;
            }

            var cmd = new MoveFleet(dto.PlayerId, 0L, dto.FleetId, dto.ToPlanetId);
            await ingress.SubmitAsync(new GameSessionId(worldId), cmd, context.RequestAborted);
            context.Response.StatusCode = 202;
        }).RequireAuthorization();

        app.MapPost("/game/session/{sessionId:guid}/commands/move-fleet", async context =>
        {
            var ingress = context.RequestServices.GetRequiredService<ICommandIngress>();
            var sessionId = context.Request.RouteValues["sessionId"] is string sid ? Guid.Parse(sid) : Guid.Empty;
            var dto = await context.Request.ReadFromJsonAsync<MoveFleetCommandDto>(context.RequestAborted);
            if (dto == null)
            {
                context.Response.StatusCode = 400;
                return;
            }
            var cmd = new MoveFleet(dto.PlayerId, dto.ClientTick, dto.FleetId, dto.ToSystemId);
            await ingress.SubmitAsync(new GameSessionId(sessionId), cmd, context.RequestAborted);
            context.Response.StatusCode = 202;
        }).RequireAuthorization();

        app.MapPost("/game/build-structure", async context =>
        {
            var commandQueue = context.RequestServices.GetRequiredService<CommandQueue>();
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
            var commandQueue = context.RequestServices.GetRequiredService<CommandQueue>();
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
            var commandQueue = context.RequestServices.GetRequiredService<CommandQueue>();
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