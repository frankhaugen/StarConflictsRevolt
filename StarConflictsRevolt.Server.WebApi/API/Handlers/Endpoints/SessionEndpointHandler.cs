using Microsoft.EntityFrameworkCore;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Sessions;
using StarConflictsRevolt.Server.WebApi.Infrastructure.Datastore;
using StarConflictsRevolt.Server.WebApi.Infrastructure.Security;

namespace StarConflictsRevolt.Server.WebApi.API.Handlers.Endpoints;

/// <summary>
///     Handles session management endpoints
/// </summary>
public static class SessionEndpointHandler
{
    public static void MapEndpoints(WebApplication app)
    {
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
            .RequireAuthorization();

        app.MapPost("/game/session", async context =>
            {
                var sessionService = context.RequestServices.GetRequiredService<SessionService>();
                var sessionManagerService = context.RequestServices.GetRequiredService<SessionAggregateManager>();
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

                var sessionId = await sessionService.CreateSessionAsync(request.SessionName, sessionType, context.RequestAborted);
                // Create a default world for the new session with planets
                var worldService = context.RequestServices.GetRequiredService<WorldService>();
                var world = await worldService.GetWorldAsync(sessionId, context.RequestAborted);
                sessionManagerService.CreateSession(sessionId, world);
                context.Response.StatusCode = 201;
                await context.Response.WriteAsJsonAsync(new SessionResponse { SessionId = sessionId, World = world.ToDto() }, context.RequestAborted);
            })
            .WithName("CreateGameSession")
            .RequireAuthorization();

        app.MapPost("/game/session/{sessionId}/join", async context =>
            {
                var sessionIdStr = context.Request.RouteValues["sessionId"]?.ToString();
                if (!Guid.TryParse(sessionIdStr, out var sessionId))
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Invalid session ID");
                    return;
                }

                var dbContext = context.RequestServices.GetRequiredService<GameDbContext>();
                var sessionManagerService = context.RequestServices.GetRequiredService<SessionAggregateManager>();
                var request = await context.Request.ReadFromJsonAsync<CreateSessionRequest>(context.RequestAborted);

                // Check if session exists
                var session = await dbContext.GetSessionAsync(sessionId, context.RequestAborted);
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
            .RequireAuthorization();

        app.MapGet("/game/sessions", async context =>
            {
                var dbContext = context.RequestServices.GetRequiredService<GameDbContext>();
                var sessions = await dbContext.Sessions
                    .Where(s => s.IsActive)
                    .OrderByDescending(s => s.Created)
                    .Select(s => new SessionDto()
                    {
                        Id = s.Id,
                        SessionName = s.SessionName,
                        Created = s.Created,
                        SessionType = s.SessionType.ToString(),
                        Ended = s.Ended,
                        IsActive = s.IsActive
                    })
                    .ToListAsync(context.RequestAborted);

                await context.Response.WriteAsJsonAsync(sessions, context.RequestAborted);
            })
            .WithName("ListGameSessions")
            .RequireAuthorization();

        app.MapGet("/game/session/{sessionId}", async context =>
            {
                var sessionIdStr = context.Request.RouteValues["sessionId"]?.ToString();
                if (!Guid.TryParse(sessionIdStr, out var sessionId))
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Invalid session ID");
                    return;
                }

                var dbContext = context.RequestServices.GetRequiredService<GameDbContext>();
                var session = await dbContext.Sessions
                    .Where(s => s.Id == sessionId && s.IsActive)
                    .Select(s => new SessionDto()
                    {
                        Id = s.Id,
                        SessionName = s.SessionName,
                        Created = s.Created,
                        SessionType = s.SessionType.ToString(),
                        Ended = s.Ended,
                        IsActive = s.IsActive
                    })
                    .FirstOrDefaultAsync(context.RequestAborted);

                if (session == null)
                {
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync("Session not found");
                    return;
                }

                await context.Response.WriteAsJsonAsync(session, context.RequestAborted);
            })
            .WithName("GetGameSession")
            .RequireAuthorization();
    }
}