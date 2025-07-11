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
using StarConflictsRevolt.Clients.Models.Authentication;
using StarConflictsRevolt.Server.WebApi.Datastore.Extensions;
using StarConflictsRevolt.Server.WebApi.Enums;
using StarConflictsRevolt.Server.WebApi.Models;
using TokenRequest = StarConflictsRevolt.Server.WebApi.Security.TokenRequest;

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
        // Remove direct endpoint mappings
        // app.MapGet("/health", ...);
        // app.MapDefaultEndpoints();
        // app.UseCors();
        // MapEndpoints(app);
        // Map SignalR hub
        // app.MapHub<WorldHub>("/gamehub");
        // Instead, call MinimalApiHelper
        MinimalApiHelper.MapMinimalApis(app);
        app.MapHub<WorldHub>("/gamehub");
        app.UseCors();
    }
} 