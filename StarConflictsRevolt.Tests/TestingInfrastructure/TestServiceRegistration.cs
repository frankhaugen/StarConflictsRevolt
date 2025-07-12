using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using StarConflictsRevolt.Clients.Http.Http;
using StarConflictsRevolt.Clients.Raylib.Services;
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

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

public static class TestServiceRegistration
{
    public static IServiceCollection AddMinimalTestServices(this IServiceCollection services)
    {
        // Only register what is needed for most tests - no background services
        services.AddSingleton<IDocumentStore>(SharedDocumentStore.Instance);
        services.AddScoped<IAsyncDocumentSession>(sp =>
            sp.GetRequiredService<IDocumentStore>().OpenAsyncSession());
        
        // Add test doubles, mocks, or minimal services here
        return services;
    }

    public static IServiceCollection AddFullIntegrationServices(this IServiceCollection services)
    {
        // Register everything needed for full integration tests
        // (copy from StartupHelper, but avoid hosted services unless needed)
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
        
        return services;
    }

    public static IServiceCollection AddTestApplicationServices(this IServiceCollection services)
    {
        // Register services needed for TestApplicationHost
        // This includes everything from AddFullIntegrationServices plus hosted services
        services.AddFullIntegrationServices();
        
        // Add hosted services for full application testing
        services.AddHostedService<GameUpdateService>();
        services.AddHostedService<AiTurnService>();
        services.AddHostedService<ProjectionService>();
        services.AddHostedService<EventBroadcastService>();
        
        // Add client services
        services.AddSingleton<IClientWorldStore, ClientWorldStore>();
        
        return services;
    }

    public static IServiceCollection AddTestApplicationServicesWithoutHostedServices(this IServiceCollection services)
    {
        // Register services needed for TestApplicationHost but WITHOUT hosted services
        // This prevents hanging during test discovery
        services.AddFullIntegrationServices();
        
        // Add client services
        services.AddSingleton<IClientWorldStore, ClientWorldStore>();
        
        return services;
    }
} 