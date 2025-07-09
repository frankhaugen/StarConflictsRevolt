using Microsoft.EntityFrameworkCore;
using Raven.Client.Documents;
using StarConflictsRevolt.Aspire.ServiceDefaults;
using StarConflictsRevolt.Server.Core;
using StarConflictsRevolt.Server.Datastore;
using StarConflictsRevolt.Server.Eventing;
using StarConflictsRevolt.Server.Services;
using Microsoft.AspNetCore.SignalR;

namespace StarConflictsRevolt.Server.WebApi;

public record TokenRequest(string ClientId, string Secret);

public static class WebApiStartupHelper
{
    public static void RegisterServices(WebApplicationBuilder builder)
    {

        // Register RavenEventStore as IEventStore
        builder.Services.AddSingleton<IEventStore, RavenEventStore>();

        // Register CommandQueue as singleton for DI
        builder.Services.AddSingleton(typeof(CommandQueue<IGameEvent>));

        // Register SessionAggregateManager
        builder.Services.AddSingleton<SessionAggregateManager>();

        // Register services
        builder.Services.AddScoped<SessionService>();
        builder.Services.AddScoped<WorldService>();
        builder.Services.AddScoped<LeaderboardService>();

        // Add HTTP client factory for Clients.Shared integration
        builder.Services.AddHttpClient();
        
        // Add SignalR
        builder.Services.AddSignalR();
        
        // Add CORS
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        builder.AddServiceDefaults();

        // Add services to the container.
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
    }
    
    public static void RegisterRavenDb(WebApplicationBuilder builder)
    {
        // Register RavenDB DocumentStore
        builder.Services.AddSingleton<IDocumentStore>(_ => new DocumentStore
        {
            Urls = [builder.Configuration.GetConnectionString("ravenDB")],
            Database = "StarConflictsRevolt"
        }.Initialize());
    }
    
    public static void RegisterGameEngineDbContext(WebApplicationBuilder builder)
    {
        // Register GameDbContext with RavenDB DocumentStore
        builder.Services.AddDbContext<GameDbContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("gameDB"));
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        });
    }
    
    public static void Configure(WebApplication app)
    {
        
        app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();
        
        // Use CORS
        app.UseCors();

        app.MapGet("/", async context =>
        {
            await context.Response.WriteAsync("Welcome to Star Conflicts Revolt API!");
        });

        // Token endpoint for client authentication
        app.MapPost("/token", async context =>
        {
            var request = await context.Request.ReadFromJsonAsync<TokenRequest>(context.RequestAborted);
            if (request == null || string.IsNullOrEmpty(request.ClientId) || string.IsNullOrEmpty(request.Secret))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid request");
                return;
            }

            // Simple token generation (replace with proper JWT implementation)
            var token = new
            {
                access_token = $"token_{request.ClientId}_{Guid.NewGuid()}",
                expires_in = 3600,
                token_type = "Bearer"
            };

            await context.Response.WriteAsJsonAsync(token, context.RequestAborted);
        });

        // SignalR hub
        app.MapHub<WorldHub>("/gamehub");

        // Leaderboard endpoints
        app.MapGet("/leaderboard/{sessionId}", async (Guid sessionId, LeaderboardService leaderboardService, CancellationToken ct) =>
        {
            var leaderboard = await leaderboardService.GetLeaderboardAsync(sessionId, ct);
            return Results.Ok(leaderboard);
        });

        app.MapGet("/leaderboard/{sessionId}/player/{playerId}", async (Guid sessionId, Guid playerId, LeaderboardService leaderboardService, CancellationToken ct) =>
        {
            var stats = await leaderboardService.GetPlayerStatsAsync(sessionId, playerId, ct);
            if (stats == null)
                return Results.NotFound();
            return Results.Ok(stats);
        });

        app.MapGet("/leaderboard/top", async (LeaderboardService leaderboardService, int count = 10, CancellationToken ct = default) =>
        {
            var topPlayers = await leaderboardService.GetTopPlayersAsync(count, ct);
            return Results.Ok(topPlayers);
        });

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

                await context.Response.WriteAsJsonAsync(world, context.RequestAborted);
            })
            .WithName("GetGameState");

        app.MapPost("/game/session", async context =>
            {
                var sessionService = context.RequestServices.GetRequiredService<SessionService>();
                var gameUpdateService = context.RequestServices.GetServices<IHostedService>()
                    .OfType<GameUpdateService>()
                    .FirstOrDefault() ?? throw new InvalidOperationException("GameUpdateService not found in the service provider");
                var sessionName = await context.Request.ReadFromJsonAsync<string>(context.RequestAborted);
                if (string.IsNullOrWhiteSpace(sessionName))
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Session name is required");
                    return;
                }

                var sessionId = await sessionService.CreateSessionAsync(sessionName, context.RequestAborted);
                // Create a default world for the new session with planets
                var worldService = context.RequestServices.GetRequiredService<WorldService>();
                var world = await worldService.GetWorldAsync(sessionId, context.RequestAborted);
                gameUpdateService.CreateSession(sessionId, world);
                context.Response.StatusCode = 201;
                await context.Response.WriteAsJsonAsync(new { SessionId = sessionId }, context.RequestAborted);
            })
            .WithName("CreateGameSession");

        app.MapPost("/game/move-fleet", async context =>
        {
            var commandQueue = context.RequestServices.GetRequiredService<CommandQueue<IGameEvent>>();
            var gameUpdateService = context.RequestServices.GetRequiredService<GameUpdateService>();
            var worldService = context.RequestServices.GetRequiredService<WorldService>();
            var dto = await context.Request.ReadFromJsonAsync<MoveFleetEvent>(context.RequestAborted);
            if (dto == null)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid MoveFleetEvent");
                return;
            }
            var worldId = context.Request.Query.ContainsKey("worldId") ? Guid.Parse(context.Request.Query["worldId"]) : Guid.Empty;
            if (!await gameUpdateService.SessionExistsAsync(worldId))
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
        });

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
            if (!Enum.TryParse<StarConflictsRevolt.Server.Core.Enums.StructureVariant>(dto.StructureType, out var _))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync($"StructureType {dto.StructureType} is not valid");
                return;
            }
            // TODO: Check player permissions/ownership if available
            commandQueue.Enqueue(worldId, dto);
            context.Response.StatusCode = 202;
        });

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
        });

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
        });

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
        });

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
        });
    }
}