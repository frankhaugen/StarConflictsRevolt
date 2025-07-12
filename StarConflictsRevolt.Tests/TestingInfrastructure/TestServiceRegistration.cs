using Microsoft.Extensions.DependencyInjection;
using StarConflictsRevolt.Clients.Raylib.Services;
using StarConflictsRevolt.Server.WebApi.Eventing;
using StarConflictsRevolt.Server.WebApi.Services;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

public static class TestServiceRegistration
{
    public static IServiceCollection AddTestApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IEventStore, RavenEventStore>();
        services.AddSingleton(typeof(CommandQueue<IGameEvent>));
        services.AddSingleton<SessionAggregateManager>();
        services.AddSingleton<WorldFactory>();
        
        services.AddScoped<SessionService>();
        services.AddScoped<WorldService>();
        services.AddScoped<LeaderboardService>();
        
        // Add SignalR
        services.AddSignalR(config =>
        {
            config.EnableDetailedErrors = true;
            config.MaximumReceiveMessageSize = 1024 * 1024; // 1 MB
        });
        
        // Add CORS
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });
        
        // Add OpenAPI
        services.AddOpenApi();
        
        // Add JWT authentication
        services.AddAuthentication("Test")
            .AddJwtBearer("Test", options =>
            {
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    RequireExpirationTime = false,
                    ValidateIssuerSigningKey = false
                };
            });
        
        // Add API versioning
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        });
        
        // Add hosted services for full application testing
        services.AddHostedService<GameUpdateService>();
        services.AddHostedService<AiTurnService>();
        services.AddHostedService<ProjectionService>();
        services.AddHostedService<EventBroadcastService>();
        
        // Add client services
        services.AddSingleton<IClientWorldStore, ClientWorldStore>();
        
        return services;
    }
} 