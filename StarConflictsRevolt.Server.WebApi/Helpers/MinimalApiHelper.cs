using StarConflictsRevolt.Server.WebApi.Services;
using StarConflictsRevolt.Server.WebApi.Eventing;
using StarConflictsRevolt.Server.WebApi.Datastore;
using StarConflictsRevolt.Server.WebApi.Security;
using StarConflictsRevolt.Server.WebApi.Enums;
using StarConflictsRevolt.Server.WebApi.Models;
using StarConflictsRevolt.Clients.Models.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace StarConflictsRevolt.Server.WebApi.Helpers;

public static class MinimalApiHelper
{
    public static void MapMinimalApis(WebApplication app)
    {
        app.MapGet("/", async context => { await context.Response.WriteAsync("Welcome to Star Conflicts Revolt API!"); });
        app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));
        app.MapGet("/health/game", () => Results.Text("Game on!"));
        // Add more health checks as needed (e.g., check DB, event store, etc.)
        // ...
        // Token endpoint for client authentication
        app.MapPost("/token", async context =>
        {
            var request = await context.Request.ReadFromJsonAsync<TokenRequest>(context.RequestAborted);
            if (request == null || string.IsNullOrEmpty(request.ClientId) || string.IsNullOrEmpty(request.ClientSecret))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid request");
                var loggerFactory = context.RequestServices.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("TokenEndpoint");
                logger.LogCritical("Invalid token request: {Request}", request);
                return;
            }
            if (request.ClientSecret != Constants.Secret)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid client secret");
                var loggerFactory = context.RequestServices.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("TokenEndpoint");
                logger.LogWarning("Invalid client secret for client {ClientId}", request.ClientId);
                return;
            }
            var gameDbContext = context.RequestServices.GetRequiredService<GameDbContext>();
            // Use direct lookup if GetClientAsync is not available
            var existingClient = gameDbContext.Clients.FirstOrDefault(c => c.Id == request.ClientId);
            if (existingClient == null)
            {
                existingClient = new Client { Id = request.ClientId, LastSeen = DateTime.UtcNow };
                gameDbContext.Clients.Add(existingClient);
                await gameDbContext.SaveChangesAsync(context.RequestAborted);
            }
            else
            {
                existingClient.LastSeen = DateTime.UtcNow;
                gameDbContext.Clients.Update(existingClient);
                await gameDbContext.SaveChangesAsync(context.RequestAborted);
            }
            var claims = new[] { new Claim("client_id", request.ClientId) };
            var now = DateTime.UtcNow;
            var jwt = new JwtSecurityToken(
                JwtConfig.Issuer,
                JwtConfig.Audience,
                claims,
                now.AddMinutes(-5),
                now.AddHours(1),
                new SigningCredentials(
                    JwtConfig.GetSymmetricSecurityKey(),
                    SecurityAlgorithms.HmacSha256)
            );
            var tokenString = new JwtSecurityTokenHandler().WriteToken(jwt);
            var token = new TokenResponse()
            {
                AccessToken = tokenString,
                TokenType = TokenType.Bearer,
                ExpiresAt = now.AddHours(1),
            };
            await context.Response.WriteAsJsonAsync(token, context.RequestAborted);
        });
        // ...
        // Leaderboard endpoints
        app.MapGet("/leaderboard/{sessionId}", async (Guid sessionId, LeaderboardService leaderboardService, CancellationToken ct) =>
        {
            var leaderboard = await leaderboardService.GetLeaderboardAsync(sessionId, ct);
            return Results.Ok(leaderboard);
        }).RequireAuthorization();

        app.MapGet("/leaderboard/{sessionId}/player/{playerId}", async (Guid sessionId, Guid playerId, LeaderboardService leaderboardService, CancellationToken ct) =>
        {
            var stats = await leaderboardService.GetPlayerStatsAsync(sessionId, playerId, ct);
            if (stats == null)
                return Results.NotFound();
            return Results.Ok(stats);
        }).RequireAuthorization();

        app.MapGet("/leaderboard/top", async (LeaderboardService leaderboardService, int count = 10, CancellationToken ct = default) =>
        {
            var topPlayers = await leaderboardService.GetTopPlayersAsync(count, ct);
            return Results.Ok(topPlayers);
        }).RequireAuthorization();

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
                await context.Response.WriteAsJsonAsync(world.ToDto(), context.RequestAborted);
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
                await context.Response.WriteAsJsonAsync(new { SessionId = sessionId, World = world.ToDto() }, context.RequestAborted);
            })
            .WithName("CreateGameSession")
            .RequireAuthorization();

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

        app.MapGet("/game/{worldId}/events", async context =>
        {
            var eventStore = context.RequestServices.GetRequiredService<IEventStore>() as RavenEventStore;
            if (eventStore == null)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Event store not available");
                return;
            }

            var worldIdStr = context.Request.RouteValues["worldId"]?.ToString();
            if (!Guid.TryParse(worldIdStr, out var worldId))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid worldId");
                return;
            }

            var events = eventStore.GetEventsForWorld(worldId);
            await context.Response.WriteAsJsonAsync(events);
        }).RequireAuthorization();

        app.MapPost("/game/{worldId}/snapshot", async context =>
        {
            var eventStore = context.RequestServices.GetRequiredService<IEventStore>() as RavenEventStore;
            if (eventStore == null)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Event store not available");
                return;
            }

            var worldIdStr = context.Request.RouteValues["worldId"]?.ToString();
            if (!Guid.TryParse(worldIdStr, out var worldId))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid worldId");
                return;
            }

            var worldState = await context.Request.ReadFromJsonAsync<object>(context.RequestAborted);
            eventStore.SnapshotWorld(worldId, worldState!);
            context.Response.StatusCode = 201;
        }).RequireAuthorization();
    }
} 