using Microsoft.EntityFrameworkCore;
using Raven.Client.Documents;
using StarConflictsRevolt.Server.Core;
using StarConflictsRevolt.Server.Datastore;
using StarConflictsRevolt.Server.Eventing;
using StarConflictsRevolt.Server.Services;

namespace StarConflictsRevolt.Server.GameEngine;

public static class GameEngineStartupHelper
{
    public static void RegisterGameEngineServices(WebApplicationBuilder builder)
    {
        // Register RavenEventStore as IEventStore
        builder.Services.AddSingleton<IEventStore, RavenEventStore>();
        
        // Register CommandQueue as singleton for DI
        builder.Services.AddSingleton(typeof(CommandQueue<IGameEvent>));

        // Register SessionAggregateManager
        builder.Services.AddSingleton<SessionAggregateManager>();

        // Register services from the Services project
        builder.Services.AddHostedService<GameUpdateService>();
        builder.Services.AddHostedService<AiTurnService>();
        builder.Services.AddHostedService<ProjectionService>();
        
        // Register Services for the game engine
        builder.Services.AddSingleton<SessionAggregate>();
        
        // Register SignalR services
        builder.Services.AddSignalR(config =>
        {
            config.EnableDetailedErrors = true;
            config.MaximumReceiveMessageSize = 1024 * 1024; // 1 MB
        });

        // Register event broadcasting service
        builder.Services.AddHostedService<EventBroadcastService>();
    }
    
    public static void RegisterGameEngineDocumentStore(WebApplicationBuilder builder)
    {
        // Register RavenDB DocumentStore
        builder.Services.AddSingleton<IDocumentStore>(_ => new DocumentStore
        {
            Urls = new[] { "http://localhost:8080" }, // TODO: Make configurable
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
    
    public static void ConfigureGameEngine(WebApplication app)
    {
        app.MapHub<WorldHub>("/gamehub");
    }
}