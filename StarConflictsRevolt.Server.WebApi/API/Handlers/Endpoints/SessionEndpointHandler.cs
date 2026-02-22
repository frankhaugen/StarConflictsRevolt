using Microsoft.AspNetCore.Authorization;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Server.Domain.Sessions;
using StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;
using StarConflictsRevolt.Server.WebApi.Infrastructure.Security;

namespace StarConflictsRevolt.Server.WebApi.API.Handlers.Endpoints;

/// <summary>
///     Handles session management endpoints.
///     In Development, session and game-state endpoints allow anonymous access so single-player works without JWT setup.
/// </summary>
public static class SessionEndpointHandler
{
    public static void MapEndpoints(WebApplication app)
    {
        var requireAuth = !app.Environment.IsDevelopment();

        app.MapGet("/game/state", async context =>
            {
                var worldService = context.RequestServices.GetRequiredService<WorldService>();
                var world = await worldService.GetWorldAsync(context.RequestAborted);
                if (world == null)
                {
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync("World not found");
                    return;
                }

                // TODO: Replace with actual player ID extraction from auth context
                Guid? playerId = null;
                var user = context.User;
                if (user.Identity?.IsAuthenticated == true)
                {
                    var idClaim = user.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "userid" || c.Type == "nameidentifier");
                    if (idClaim != null && Guid.TryParse(idClaim.Value, out var parsedId))
                        playerId = parsedId;
                }
                // For now, fallback to first player if not found
                if (!playerId.HasValue && world.Players.Count > 0)
                    playerId = world.Players[0].PlayerId;

                await context.Response.WriteAsJsonAsync(world.ToDto(playerId), context.RequestAborted);
            })
            .WithName("GetGameState")
            .RequireAuthorizationIf(requireAuth);

        app.MapPost("/game/session", async context =>
            {
                var sessionService = context.RequestServices.GetRequiredService<SessionService>();
                var sessionManagerService = context.RequestServices.GetRequiredService<SessionAggregateManager>();
                var worldFactory = context.RequestServices.GetRequiredService<WorldFactory>();
                var persistence = context.RequestServices.GetRequiredService<IGamePersistence>();
                var request = await context.Request.ReadFromJsonAsync<CreateSessionRequest>(context.RequestAborted);
                if (request == null || string.IsNullOrWhiteSpace(request.SessionName))
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Session name is required");
                    return;
                }

                var sessionType = request.SessionType?.ToLower() switch
                {
                    "singleplayer" => SessionType.SinglePlayer,
                    "multiplayer" => SessionType.Multiplayer,
                    _ => SessionType.Multiplayer // Default to multiplayer
                };

                // Player tracking: prefer request body, then auth "sub" or "nameidentifier"
                var clientId = request.ClientId;
                if (string.IsNullOrWhiteSpace(clientId) && context.User.Identity?.IsAuthenticated == true)
                {
                    var idClaim = context.User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "nameidentifier" || c.Type == "userid");
                    if (!string.IsNullOrWhiteSpace(idClaim?.Value))
                        clientId = idClaim.Value;
                }

                // Single-player resume: return existing session for this player instead of creating a new world
                if (sessionType == SessionType.SinglePlayer && !string.IsNullOrWhiteSpace(clientId))
                {
                    var existingSessions = await persistence.GetActiveSessionsByPlayerAsync(clientId, context.RequestAborted);
                    var existing = existingSessions
                        .Where(s => s.SessionType == SessionType.SinglePlayer)
                        .OrderByDescending(s => s.Created)
                        .FirstOrDefault();
                    if (existing != null)
                    {
                        var world = sessionManagerService.HasAggregate(existing.Id)
                            ? sessionManagerService.GetAggregate(existing.Id)!.World
                            : worldFactory.CreateDefaultWorld();
                        if (world.Id != existing.Id)
                        {
                            world.Id = existing.Id;
                            if (!sessionManagerService.HasAggregate(existing.Id))
                                sessionManagerService.CreateSession(existing.Id, world);
                        }
                        context.Response.StatusCode = 200;
                        await context.Response.WriteAsJsonAsync(new SessionResponse { SessionId = existing.Id, World = world.ToDto() }, context.RequestAborted);
                        return;
                    }
                }

                var sessionId = await sessionService.CreateSessionAsync(request.SessionName, sessionType, string.IsNullOrWhiteSpace(clientId) ? null : clientId, context.RequestAborted);
                var newWorld = worldFactory.CreateDefaultWorld();
                newWorld.Id = sessionId;
                sessionManagerService.CreateSession(sessionId, newWorld);
                context.Response.StatusCode = 201;
                await context.Response.WriteAsJsonAsync(new SessionResponse { SessionId = sessionId, World = newWorld.ToDto() }, context.RequestAborted);
            })
            .WithName("CreateGameSession")
            .RequireAuthorizationIf(requireAuth);

        app.MapPost("/game/session/{sessionId}/join", async context =>
            {
                var sessionIdStr = context.Request.RouteValues["sessionId"]?.ToString();
                if (!Guid.TryParse(sessionIdStr, out var sessionId))
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Invalid session ID");
                    return;
                }

                var persistence = context.RequestServices.GetRequiredService<IGamePersistence>();
                var sessionManagerService = context.RequestServices.GetRequiredService<SessionAggregateManager>();
                var request = await context.Request.ReadFromJsonAsync<CreateSessionRequest>(context.RequestAborted);

                // Check if session exists
                var session = await persistence.GetSessionAsync(sessionId, context.RequestAborted);
                if (session == null)
                {
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync("Session not found");
                    return;
                }

                // Get the world for this session
                var worldService = context.RequestServices.GetRequiredService<WorldService>();
                var world = await worldService.GetWorldAsync(sessionId, context.RequestAborted);

                // Ensure session aggregate exists
                if (!sessionManagerService.HasAggregate(sessionId)) sessionManagerService.CreateSession(sessionId, world);

                context.Response.StatusCode = 200;
                await context.Response.WriteAsJsonAsync(new SessionResponse { SessionId = sessionId, World = world.ToDto() }, context.RequestAborted);
            })
            .WithName("JoinGameSession")
            .RequireAuthorizationIf(requireAuth);

        app.MapGet("/game/sessions", async context =>
            {
                var persistence = context.RequestServices.GetRequiredService<IGamePersistence>();
                var sessions = await persistence.ListActiveSessionsAsync(context.RequestAborted);
                var dtos = sessions.Select(s => new SessionDto
                {
                    Id = s.Id,
                    SessionName = s.SessionName,
                    Created = s.Created,
                    SessionType = s.SessionType.ToString(),
                    Ended = s.Ended,
                    IsActive = s.IsActive
                }).ToList();
                await context.Response.WriteAsJsonAsync(dtos, context.RequestAborted);
            })
            .WithName("ListGameSessions")
            .RequireAuthorizationIf(requireAuth);

        app.MapGet("/game/session/{sessionId}", async context =>
            {
                var sessionIdStr = context.Request.RouteValues["sessionId"]?.ToString();
                if (!Guid.TryParse(sessionIdStr, out var sessionId))
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Invalid session ID");
                    return;
                }

                var persistence = context.RequestServices.GetRequiredService<IGamePersistence>();
                var s = await persistence.GetSessionAsync(sessionId, context.RequestAborted);
                var session = s != null && s.IsActive
                    ? new SessionDto { Id = s.Id, SessionName = s.SessionName, Created = s.Created, SessionType = s.SessionType.ToString(), Ended = s.Ended, IsActive = s.IsActive }
                    : null;

                if (session == null)
                {
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync("Session not found");
                    return;
                }

                await context.Response.WriteAsJsonAsync(session, context.RequestAborted);
            })
            .WithName("GetGameSession")
            .RequireAuthorizationIf(requireAuth);

        app.MapDelete("/game/session/{sessionId}", async context =>
            {
                var sessionIdStr = context.Request.RouteValues["sessionId"]?.ToString();
                if (!Guid.TryParse(sessionIdStr, out var sessionId))
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Invalid session ID");
                    return;
                }

                var persistence = context.RequestServices.GetRequiredService<IGamePersistence>();
                var sessionManagerService = context.RequestServices.GetRequiredService<SessionAggregateManager>();

                var session = await persistence.GetSessionAsync(sessionId, context.RequestAborted);
                if (session == null)
                {
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync("Session not found");
                    return;
                }

                await persistence.EndSessionAsync(sessionId, context.RequestAborted);
                sessionManagerService.RemoveAggregate(sessionId);

                context.Response.StatusCode = 204;
            })
            .WithName("DeleteGameSession")
            .RequireAuthorizationIf(requireAuth);
    }

    private static IEndpointConventionBuilder RequireAuthorizationIf(this IEndpointConventionBuilder builder, bool require)
    {
        if (require) builder.RequireAuthorization();
        return builder;
    }
}