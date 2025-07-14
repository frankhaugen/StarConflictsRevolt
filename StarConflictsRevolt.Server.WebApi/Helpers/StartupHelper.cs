using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Raven.Client.Documents;
using StarConflictsRevolt.Server.WebApi.Datastore;
using StarConflictsRevolt.Server.WebApi.Eventing;
using StarConflictsRevolt.Server.WebApi.Security;
using StarConflictsRevolt.Server.WebApi.Services;
using StarConflictsRevolt.Server.WebApi.Services.AiStrategies;
using StarConflictsRevolt.Server.WebApi.Models;

namespace StarConflictsRevolt.Server.WebApi.Helpers;

public static class StartupHelper
{
    // Register all core services except databases. Call RegisterRavenDb/RegisterGameDbContext explicitly in app or test setup.
    public static void RegisterAllServices(WebApplicationBuilder builder)
    {
        // Set minimum log level to Debug for all loggers
        builder.Logging.SetMinimumLevel(LogLevel.Debug);
        // Add core services
        builder.Services.AddSingleton<IEventStore, RavenEventStore>();
        builder.Services.AddSingleton(typeof(CommandQueue<IGameEvent>));
        builder.Services.AddSingleton<SessionAggregateManager>();
        builder.Services.AddSingleton<WorldFactory>();
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
        builder.Services.AddScoped<GameSetupService>();
        builder.Services.AddScoped<GameContentService>();

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
        MinimalApiHelper.MapMinimalApis(app);
        app.MapOpenApi();
        app.MapHub<WorldHub>("/gamehub");
        app.UseCors();
    }
}