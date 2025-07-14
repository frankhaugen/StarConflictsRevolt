using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Raven.Client.Documents;
using StarConflictsRevolt.Server.WebApi.API.Handlers.Endpoints;
using StarConflictsRevolt.Server.WebApi.Application.Services.AI;
using StarConflictsRevolt.Server.WebApi.Application.Services.Combat;
using StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;
using StarConflictsRevolt.Server.WebApi.Core.Domain.AI;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Events;
using StarConflictsRevolt.Server.WebApi.Infrastructure.Datastore;
using StarConflictsRevolt.Server.WebApi.Infrastructure.Security;
using Frank.PulseFlow;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Combat;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Planets;
using StarConflictsRevolt.Server.WebApi.Infrastructure.MessageFlows;

namespace StarConflictsRevolt.Server.WebApi.Infrastructure.Configuration;

public static class StartupHelper
{
    // Register all core services except databases. Call RegisterRavenDb/RegisterGameDbContext explicitly in app or test setup.
    public static void RegisterAllServices(WebApplicationBuilder builder)
    {
        // Set minimum log level to Debug for all loggers
        builder.Logging.SetMinimumLevel(LogLevel.Debug);
        
        // Add Frank.PulseFlow for GameTick pulse
        builder.Services.AddPulseFlow<GameTickMessageFlow>();
        
        // Add core services
        builder.Services.AddSingleton<IEventStore, RavenEventStore>();
        builder.Services.AddSingleton<SessionAggregateManager>();
        builder.Services.AddSingleton<WorldFactory>();
        builder.Services.AddSingleton<GameUpdateService>();
        
        // Register AI memory bank
        builder.Services.AddSingleton<AiMemoryBank>();

        // Register AI strategies
        builder.Services.AddSingleton<DefaultAiStrategy>();
        builder.Services.AddSingleton<AggressiveAiStrategy>();
        builder.Services.AddSingleton<EconomicAiStrategy>();
        builder.Services.AddSingleton<DefensiveAiStrategy>();
        builder.Services.AddSingleton<BalancedAiStrategy>();

        // Register default AI strategy
        builder.Services.AddSingleton<IAiStrategy, DefaultAiStrategy>();
        builder.Services.AddSingleton<AiTurnService>();
        builder.Services.AddScoped<GameSetupService>();
        builder.Services.AddScoped<GameContentService>();

        // Register combat services
        builder.Services.AddScoped<ICombatSimulator, CombatSimulatorService>();
        builder.Services.AddScoped<IFleetCombatSimulator, FleetCombatSimulator>();
        builder.Services.AddScoped<IPlanetaryCombatSimulator, PlanetaryCombatSimulator>();
        builder.Services.AddScoped<IDeathStarRunSimulator, DeathStarRunSimulator>();
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

        // Register hosted services
        builder.Services.AddHostedService<GameTickService>();
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
        Console.WriteLine("Registering RavenDB document store...");
        var ravenDbConnectionString = builder.Configuration.GetConnectionString("ravenDb");
        string ravenDbUrl;
        if (ravenDbConnectionString?.StartsWith("URL=") == true)
            ravenDbUrl = ravenDbConnectionString.Substring(4);
        else
            ravenDbUrl = ravenDbConnectionString ?? "http://localhost:8080";

        var documentStore = new DocumentStore
        {
            Urls = new[] { ravenDbUrl },
            Database = "StarConflictsRevolt"
        }.Initialize();

        builder.Services.AddSingleton<IDocumentStore>(documentStore);

        Console.WriteLine("Registering RavenDB document store completed.");
    }

    public static void RegisterGameDbContext(WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<GameDbContext>(options =>
        {
            var rawConnectionString = builder.Configuration.GetConnectionString("gameDb");
            var connectionStringBuilder = new SqlConnectionStringBuilder(rawConnectionString)
            {
                // Ensure the connection string is valid and not using default placeholder
                ApplicationName = "StarConflictsRevolt.Server.WebApi",
                MultipleActiveResultSets = true,
                InitialCatalog = "StarConflictsRevolt"
            };
            var connectionString = connectionStringBuilder.ConnectionString;
            options.UseSqlServer(connectionString);
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        });
    }

    public static async Task ConfigureAsync(WebApplication app)
    {
        // Run migrations
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<GameDbContext>();

            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                // Clean the database if it exists
                await dbContext.Database.EnsureDeletedAsync();
                // await dbContext.Database.EnsureCreatedAsync();

                Console.WriteLine("Applying pending migrations...");
                await dbContext.Database.MigrateAsync();
                Console.WriteLine("Migrations applied successfully.");
            }
            else
            {
                Console.WriteLine("No pending migrations found.");
            }
        }

        app.UseAuthentication();
        app.UseAuthorization();
        ApiEndpointHandler.MapAllEndpoints(app);
        app.MapOpenApi();
        app.MapHub<WorldHub>("/gamehub");
        app.UseCors();
    }
}

public class MissionSimulator : IMissionSimulator
{
    /// <inheritdoc />
    public CombatResult SimulateMission(Mission mission, Character agent, Planet target)
    {
        return null;
    }

    /// <inheritdoc />
    public double CalculateMissionDifficulty(Mission mission, Planet target, Character agent)
    {
        return 0;
    }

    /// <inheritdoc />
    public double CalculateSkillBonus(Character agent, MissionType missionType)
    {
        return 0;
    }

    /// <inheritdoc />
    public double CalculateEnvironmentalModifier(Planet target)
    {
        return 0;
    }

    /// <inheritdoc />
    public double CalculateSuccessChance(int difficulty, double skillBonus, double environmentalModifier)
    {
        return 0;
    }

    /// <inheritdoc />
    public List<MissionReward> CalculateRewards(Mission mission, bool success, Character agent)
    {
        return null;
    }

    /// <inheritdoc />
    public List<MissionConsequence> ApplyMissionConsequences(Mission mission, bool success, Planet target)
    {
        return null;
    }
}