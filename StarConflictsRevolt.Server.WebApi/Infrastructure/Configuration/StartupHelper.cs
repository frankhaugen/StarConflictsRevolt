using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using StarConflictsRevolt.Server.EventStorage.RavenDB;
using StarConflictsRevolt.Server.WebApi.API.Handlers.Endpoints;
using StarConflictsRevolt.Server.AI;
using StarConflictsRevolt.Server.Combat;
using StarConflictsRevolt.Server.Simulation.Engine;
using StarConflictsRevolt.Server.Domain.AI;
using StarConflictsRevolt.Server.Domain.Engine;
using StarConflictsRevolt.Server.Application.Services.Gameplay;
using StarConflictsRevolt.Server.EventStorage.Abstractions;
using StarConflictsRevolt.Server.Storage.Abstractions;
using StarConflictsRevolt.Server.Storage.LiteDb;
using StarConflictsRevolt.Server.WebApi.Infrastructure.Datastore.LiteDb;
using StarConflictsRevolt.Server.WebApi.Infrastructure.Security;
using StarConflictsRevolt.Server.WebApi.Infrastructure.Transport;

namespace StarConflictsRevolt.Server.WebApi.Infrastructure.Configuration;

public static class StartupHelper
{
    // Register all core services except databases. Call RegisterRavenDb/RegisterLiteDb explicitly in app or test setup.
    public static void RegisterAllServices(WebApplicationBuilder builder)
    {
        // Set minimum log level to Debug for all loggers
        builder.Logging.SetMinimumLevel(LogLevel.Debug);
        
        // Transport: tick fan-out to in-process listeners and SignalR (see docs/reference/transport-layer-spec.md)
        builder.Services.AddSingleton<ITickListener, AiTurnTickListener>();
        builder.Services.AddSingleton<ITickListener, GameUpdateTickListener>();
        builder.Services.AddSingleton<TickTransport>();
        builder.Services.AddSingleton<ITickPublisher>(sp => sp.GetRequiredService<TickTransport>());

        // Add core services (IEventStore is registered by RegisterRavenDb via AddRavenDbEventStorage; call RegisterRavenDb before RegisterAllServices)
        builder.Services.AddSingleton<SessionAggregateManager>();
        builder.Services.AddSingleton<WorldFactory>();
        builder.Services.AddSingleton<ICommandQueue, CommandQueueChannel>();
        builder.Services.AddSingleton<ICommandIngress, CommandIngress>();
        builder.Services.AddSingleton<IGameSim, GameSim>();
        builder.Services.AddSingleton<WorldEngine>();
        builder.Services.AddSingleton<GameUpdateService>();
        
        // Register AI memory bank
        builder.Services.AddSingleton<AiMemoryBank>();

        // Register AI strategies
        builder.Services.AddSingleton<DefaultAiStrategy>();
        builder.Services.AddSingleton<AggressiveAiStrategy>();
        builder.Services.AddSingleton<EconomicAiStrategy>();
        builder.Services.AddSingleton<DefensiveAiStrategy>();
        builder.Services.AddSingleton<BalancedAiStrategy>();

        // Register AI difficulty service
        builder.Services.AddScoped<IAiDifficultyService, AiDifficultyService>();

        // Register default AI strategy
        builder.Services.AddSingleton<IAiStrategy, DefaultAiStrategy>();
        builder.Services.AddSingleton<AiTurnService>();
        builder.Services.AddScoped<GameSetupService>();
        builder.Services.AddScoped<GameContentService>();
        builder.Services.AddScoped<IGameScenarioService, GameScenarioService>();

        // Register combat services
        builder.Services.AddScoped<ICombatSimulator, CombatSimulatorService>();
        builder.Services.AddScoped<IFleetCombatSimulator, FleetCombatSimulator>();
        builder.Services.AddScoped<IPlanetaryCombatSimulator, PlanetaryCombatSimulator>();
        builder.Services.AddScoped<IDeathStarRunSimulator, DeathStarRunSimulator>();
        builder.Services.AddScoped<IDeathStarRunCombatService, DeathStarRunCombatService>();
        builder.Services.AddScoped<IMissionSimulator, MissionSimulator>();
        builder.Services.AddScoped<ITargetSelector, TargetSelector>();
        builder.Services.AddScoped<IAttackResolver, AttackResolver>();
        builder.Services.AddScoped<ICombatEndChecker, CombatEndChecker>();
        builder.Services.AddScoped<ICombatResultCalculator, CombatResultCalculator>();

        builder.Services.AddScoped<SessionService>();
        builder.Services.AddScoped<WorldService>();
        builder.Services.AddScoped<LeaderboardService>();

        // Register CommandQueue
        builder.Services.AddSingleton<CommandQueue>();

        // Register hosted services (single event path: EventBroadcastService pushes to SignalR; ProjectionService removed)
        builder.Services.AddHostedService<GameTickService>();
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
    /// <summary>
    /// Registers RavenDB event storage (document store + <see cref="IEventStore"/>). Call before <see cref="RegisterAllServices"/>.
    /// </summary>
    public static void RegisterRavenDb(WebApplicationBuilder builder)
    {
        Console.WriteLine("Registering RavenDB event storage...");
        builder.Services.AddRavenDbEventStorage(builder.Configuration);
        Console.WriteLine("Registering RavenDB event storage completed.");
    }

    /// <summary>
    /// Registers storage (LiteDB provider) and session/client persistence.
    /// Uses <see cref="AddStorage"/> and <see cref="LiteDbStorageExtensions.AddLiteDbProvider"/>; exposes <see cref="IRepository{T}"/> and <see cref="IGamePersistence"/>.
    /// Events remain in RavenDB via IEventStore.
    /// </summary>
    public static void RegisterLiteDb(WebApplicationBuilder builder)
    {
        var pathOrConnection = builder.Configuration.GetConnectionString("liteDb")
            ?? builder.Configuration["LiteDb:FileName"]
            ?? "game.db";
        if (pathOrConnection.StartsWith("Filename=", StringComparison.OrdinalIgnoreCase))
            pathOrConnection = pathOrConnection.Substring("Filename=".Length).Trim();

        builder.Services.AddStorage(opt =>
        {
            opt.AddLiteDbProvider(o =>
            {
                o.DatabasePath = pathOrConnection;
            });
        });
        builder.Services.AddSingleton<IGamePersistence, LiteDbGamePersistence>();
    }

    public static Task ConfigureAsync(WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        ApiEndpointHandler.MapAllEndpoints(app);
        app.MapOpenApi();
        app.MapHub<WorldHub>("/gamehub");
        app.MapHub<GameHub>("/commandhub");
        app.UseCors();
        return Task.CompletedTask;
    }
}

