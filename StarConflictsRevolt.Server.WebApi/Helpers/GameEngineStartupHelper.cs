using Microsoft.EntityFrameworkCore;
using Raven.Client.Documents;
using StarConflictsRevolt.Server.WebApi.Datastore;
using StarConflictsRevolt.Server.WebApi.Eventing;
using StarConflictsRevolt.Server.WebApi.Services;

namespace StarConflictsRevolt.Server.WebApi.Helpers;

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
        
        builder.Services.AddSingleton<SessionManagerService>();
        
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
        // Register RavenDB DocumentStore using Aspire service discovery
        builder.Services.AddSingleton<IDocumentStore>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var ravenDbConnectionString = configuration.GetConnectionString("ravenDb");
            
            // Parse the connection string to extract the URL
            string ravenDbUrl;
            if (ravenDbConnectionString?.StartsWith("URL=") == true)
            {
                ravenDbUrl = ravenDbConnectionString.Substring(4); // Remove "URL=" prefix
            }
            else
            {
                ravenDbUrl = ravenDbConnectionString ?? "http://localhost:8080";
            }
            
            return new DocumentStore
            {
                Urls = new[] { ravenDbUrl },
                Database = "StarConflictsRevolt"
            }.Initialize();
        });
    }
    
    public static void RegisterGameEngineDbContext(WebApplicationBuilder builder)
    {
        // Register GameDbContext with SQL Server using Aspire service discovery
        builder.Services.AddDbContext<GameDbContext>(options =>
        {
            var connectionString = builder.Configuration.GetConnectionString("gameDb");
            options.UseSqlServer(connectionString);
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        });
    }
    
    public static void ConfigureGameEngine(WebApplication app)
    {
        app.MapHub<WorldHub>("/gamehub");
    }
}