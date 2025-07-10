using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Raven.Client.Documents;
using StarConflictsRevolt.Server.WebApi.Datastore;
using StarConflictsRevolt.Server.WebApi.Eventing;
using StarConflictsRevolt.Server.WebApi.Services;
using StarConflictsRevolt.Server.WebApi.Security;
using StarConflictsRevolt.Aspire.ServiceDefaults;
using StarConflictsRevolt.Server.WebApi.Enums;

namespace StarConflictsRevolt.Server.WebApi.Helpers;

public static class StartupHelper
{
    // Register all core services except databases. Call RegisterRavenDb/RegisterGameDbContext explicitly in app or test setup.
    public static void RegisterAllServices(WebApplicationBuilder builder)
    {
        // Add ServiceDefaults for Aspire
        builder.AddServiceDefaults();
        
        // Add core services
        builder.Services.AddSingleton<IEventStore, RavenEventStore>();
        builder.Services.AddSingleton(typeof(CommandQueue<IGameEvent>));
        builder.Services.AddSingleton<SessionAggregateManager>();
        builder.Services.AddSingleton<WorldFactory>();
        
        builder.Services.AddScoped<SessionService>();
        builder.Services.AddScoped<WorldService>();
        builder.Services.AddScoped<LeaderboardService>();
        
        builder.Services.AddHostedService<GameUpdateService>();
        builder.Services.AddHostedService<AiTurnService>();
        builder.Services.AddHostedService<ProjectionService>();
        builder.Services.AddHostedService<EventBroadcastService>();
        
        // Add SignalR
        builder.Services.AddSignalR(config =>
        {
            config.EnableDetailedErrors = true;
            config.MaximumReceiveMessageSize = 1024 * 1024; // 1 MB
        });
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
        // Add OpenAPI
        builder.Services.AddOpenApi();
        
        // Add JWT authentication
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = JwtConfig.Issuer,
                    ValidAudience = JwtConfig.Audience,
                    IssuerSigningKey = JwtConfig.GetSymmetricSecurityKey()
                };
            });
        // Add API versioning
        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        });
    }

    // --- Database registration methods ---
    public static void RegisterRavenDb(WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IDocumentStore>(sp =>
        {
            var ravenDbConnectionString = builder.Configuration.GetConnectionString("ravenDb");
            string ravenDbUrl;
            if (ravenDbConnectionString?.StartsWith("URL=") == true)
                ravenDbUrl = ravenDbConnectionString.Substring(4);
            else
                ravenDbUrl = ravenDbConnectionString ?? "http://localhost:8080";
            return new DocumentStore
            {
                Urls = new[] { ravenDbUrl },
                Database = "StarConflictsRevolt"
            }.Initialize();
        });
    }
    
    public static void RegisterGameDbContext(WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<GameDbContext>(options =>
        {
            var connectionString = builder.Configuration.GetConnectionString("gameDb");
            options.UseSqlServer(connectionString);
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        });
    }

    public static void Configure(WebApplication app)
    {
        // Ensure database is created with retry logic
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<GameDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("gameDb");
            if (!string.IsNullOrEmpty(connectionString))
            {
                var safeConnectionString = connectionString.Replace("Password=", "Password=***");
                logger.LogInformation("Using connection string: {ConnectionString}", safeConnectionString);
                if (connectionString == "SET_BY_ASPIRE_OR_ENVIRONMENT")
                {
                    logger.LogWarning("The gameDb connection string is not set by Aspire or environment. Database will not work.");
                }
            }
            else
            {
                logger.LogWarning("No connection string found for 'gameDb'");
            }
            var maxRetries = 2;
            var retryDelay = TimeSpan.FromSeconds(1);
            for (var attempt = 1; attempt <= maxRetries; attempt++)
                try
                {
                    logger.LogInformation("Attempting to ensure database is created (attempt {Attempt}/{MaxRetries})", attempt, maxRetries);
                    db.Database.EnsureCreated();
                    logger.LogInformation("Database created successfully");
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to create database on attempt {Attempt}/{MaxRetries}", attempt, maxRetries);
                    if (attempt == maxRetries)
                    {
                        logger.LogError(ex, "Failed to create database after {MaxRetries} attempts. Application will continue but database operations may fail.", maxRetries);
                        break;
                    }
                    Thread.Sleep(retryDelay);
                }
        }
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));
        
        app.MapDefaultEndpoints();
        if (app.Environment.IsDevelopment()) app.MapOpenApi();
        app.UseCors();
        MapEndpoints(app);
        
        // (Other endpoint mappings remain unchanged)
        // Map SignalR hub
        app.MapHub<WorldHub>("/gamehub");
    }

    private static void MapEndpoints(WebApplication app)
    {
        app.MapGet("/", async context => { await context.Response.WriteAsync("Welcome to Star Conflicts Revolt API!"); });

        // Token endpoint for client authentication
        app.MapPost("/token", async context =>
        {
            var request = await context.Request.ReadFromJsonAsync<TokenRequest>(context.RequestAborted);
            if (request == null || string.IsNullOrEmpty(request.ClientId) || string.IsNullOrEmpty(request.Secret))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid request");
                var loggerFactory = context.RequestServices.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("TokenEndpoint");
                logger.LogCritical("Invalid token request: {Request}", request);
                return;
            }

            // TODO: Validate clientId/secret against your store
            // For demo, accept any non-empty

            var claims = new[]
            {
                new Claim("client_id", request.ClientId)
            };
            var now = DateTime.UtcNow;
            var jwt = new JwtSecurityToken(
                JwtConfig.Issuer,
                JwtConfig.Audience,
                claims,
                now,
                now.AddHours(1),
                new SigningCredentials(
                    JwtConfig.GetSymmetricSecurityKey(),
                    SecurityAlgorithms.HmacSha256)
            );
            var tokenString = new JwtSecurityTokenHandler().WriteToken(jwt);
            var token = new
            {
                access_token = tokenString,
                expires_in = 3600,
                token_type = "Bearer"
            };
            await context.Response.WriteAsJsonAsync(token, context.RequestAborted);
        });

        // Leaderboard endpoints
        // Protect endpoints with authentication
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