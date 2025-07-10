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

namespace StarConflictsRevolt.Server.WebApi.Helpers;

public static class StartupHelper
{
    // Register all core services except databases. Call RegisterRavenDb/RegisterGameDbContext explicitly in app or test setup.
    public static void RegisterAllServices(WebApplicationBuilder builder)
    {
        RegisterAllServices(builder.Services, builder.Configuration);
        // Do NOT call RegisterRavenDb or RegisterGameDbContext here!
        // App (Program.cs) or test setup must call the appropriate DB registration explicitly.
        // Add HTTP client factory for Clients.Shared integration
        builder.Services.AddHttpClient();
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
        // Add ServiceDefaults for Aspire
        builder.AddServiceDefaults();
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

    public static void RegisterAllServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register RavenEventStore as IEventStore
        services.AddSingleton<IEventStore, RavenEventStore>();
        // Register CommandQueue as singleton for DI
        services.AddSingleton(typeof(CommandQueue<IGameEvent>));
        // Register SessionAggregateManager
        services.AddSingleton<SessionAggregateManager>();
        // Register WorldFactory
        services.AddSingleton<WorldFactory>();
        // Register services
        services.AddScoped<SessionService>();
        services.AddScoped<WorldService>();
        services.AddScoped<LeaderboardService>();
        // Register hosted services
        services.AddHostedService<GameUpdateService>();
        services.AddHostedService<AiTurnService>();
        services.AddHostedService<ProjectionService>();
        services.AddHostedService<EventBroadcastService>();
        // Do NOT register databases here; call RegisterRavenDb/RegisterGameDbContext as needed.
    }

    // --- Database registration methods ---
    public static void RegisterRavenDb(WebApplicationBuilder builder)
    {
        RegisterRavenDb(builder.Services, builder.Configuration);
    }
    public static void RegisterRavenDb(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IDocumentStore>(sp =>
        {
            var ravenDbConnectionString = configuration.GetConnectionString("ravenDb");
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
        RegisterGameDbContext(builder.Services, builder.Configuration);
    }
    public static void RegisterGameDbContext(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<GameDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("gameDb");
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
            }
            else
            {
                logger.LogWarning("No connection string found for 'gameDb'");
            }
            var maxRetries = 5;
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
        app.MapGet("/", async context => { await context.Response.WriteAsync("Welcome to Star Conflicts Revolt API!"); });
        // (Other endpoint mappings remain unchanged)
        // Map SignalR hub
        app.MapHub<WorldHub>("/gamehub");
    }
} 