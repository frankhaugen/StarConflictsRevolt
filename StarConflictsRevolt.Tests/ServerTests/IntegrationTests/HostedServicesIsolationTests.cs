using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StarConflictsRevolt.Server.WebApi.Datastore;
using StarConflictsRevolt.Server.WebApi.Services;
using StarConflictsRevolt.Server.WebApi.Eventing;

namespace StarConflictsRevolt.Tests.ServerTests.IntegrationTests;

// Mock implementations to avoid RavenDB dependencies
public class MockEventStore : IEventStore
{
    private readonly List<EventEnvelope> _events = new();
    
    public Task PublishAsync(Guid worldId, IGameEvent gameEvent)
    {
        _events.Add(new EventEnvelope(worldId, gameEvent, DateTime.UtcNow));
        return Task.CompletedTask;
    }

    public Task SubscribeAsync(Func<EventEnvelope, Task> handler, CancellationToken cancellationToken)
    {
        // For testing, we can just simulate subscription by invoking the handler immediately
        foreach (var gameEvent in _events)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                handler(gameEvent).GetAwaiter().GetResult();
            }
        }
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        // No resources to dispose in this mock
        return ValueTask.CompletedTask;
    }
    
    public IEnumerable<EventEnvelope> GetEvents(Guid worldId) => _events.Where(e => e.WorldId == worldId);
    
    public Task ClearEventsAsync(Guid worldId)
    {
        _events.RemoveAll(e => e.WorldId == worldId);
        return Task.CompletedTask;
    }
}

public class HostedServicesIsolationTests
{
    [Test]
    [Timeout(30_000)]
    public async Task GameUpdateService_StartsAndStops_WithoutHanging(CancellationToken cancellationToken)
    {
        // Create a minimal service provider with only the required dependencies for GameUpdateService
        var services = new ServiceCollection();
        
        // Add logging - this will provide ILogger<T> automatically
        services.AddLogging(builder => builder.AddConsole());
        
        // Add SignalR (required for IHubContext<WorldHub>)
        services.AddSignalR();
        
        // Add the service under test
        services.AddHostedService<GameUpdateService>();
        
        // Add minimal dependencies that GameUpdateService needs
        services.AddSingleton<SessionAggregateManager>();
        services.AddSingleton<WorldService>();
        services.AddSingleton<SessionService>();
        services.AddSingleton<IEventStore, MockEventStore>();
        services.AddSingleton(typeof(CommandQueue<IGameEvent>));
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Get the hosted service
        var hostedService = serviceProvider.GetRequiredService<IHostedService>();
        
        // Start the service
        await hostedService.StartAsync(cancellationToken);
        
        // Wait a bit to see if it hangs
        await Task.Delay(1000, cancellationToken);
        
        // Stop the service
        await hostedService.StopAsync(cancellationToken);
        
        // If we get here, the service didn't hang
        await Assert.That(true).IsTrue();
    }

    [Test]
    [Timeout(60_000)]
    public async Task AiTurnService_StartsAndStops_WithoutHanging(CancellationToken cancellationToken)
    {
        // Create a minimal service provider with only the required dependencies for AiTurnService
        var services = new ServiceCollection();
        
        // Add db context
        services.AddDbContext<GameDbContext>(options => options.UseSqlite("Data Source=:memory:;Mode=Memory;Cache=Shared"));
        
        // Add logging - this will provide ILogger<T> automatically
        services.AddLogging(builder => builder.AddConsole());
        
        // Add the service under test
        services.AddHostedService<AiTurnService>();
        
        // Add minimal dependencies that AiTurnService needs
        services.AddSingleton<SessionAggregateManager>();
        services.AddSingleton<WorldService>();
        services.AddSingleton<SessionService>();
        services.AddSingleton<IEventStore, MockEventStore>();
        services.AddSingleton(typeof(CommandQueue<IGameEvent>));
        services.AddSingleton<IAiStrategy, DefaultAiStrategy>();
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Ensure the database is created
        using (var scope = serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<GameDbContext>();

            try
            {
                await dbContext.Database.EnsureCreatedAsync(cancellationToken);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        
        // Get the hosted service
        var hostedService = serviceProvider.GetRequiredService<IHostedService>();
        
        // Start the service
        await hostedService.StartAsync(cancellationToken);
        
        // Wait a bit to see if it hangs
        await Task.Delay(1000, cancellationToken);
        
        // Stop the service
        await hostedService.StopAsync(cancellationToken);
        
        // If we get here, the service didn't hang
        await Assert.That(true).IsTrue();
    }

    [Test]
    [Timeout(30_000)]
    public async Task ProjectionService_StartsAndStops_WithoutHanging(CancellationToken cancellationToken)
    {
        // Create a minimal service provider with only the required dependencies for ProjectionService
        var services = new ServiceCollection();
        
        // Add logging - this will provide ILogger<T> automatically
        services.AddLogging(builder => builder.AddConsole());
        
        // Add the service under test
        services.AddHostedService<ProjectionService>();
        
        // Add minimal dependencies that ProjectionService needs
        services.AddSingleton<SessionAggregateManager>();
        services.AddSingleton<WorldService>();
        services.AddSingleton<SessionService>();
        services.AddSingleton<IEventStore, MockEventStore>();
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Get the hosted service
        var hostedService = serviceProvider.GetRequiredService<IHostedService>();
        
        // Start the service
        await hostedService.StartAsync(cancellationToken);
        
        // Wait a bit to see if it hangs
        await Task.Delay(1000, cancellationToken);
        
        // Stop the service
        await hostedService.StopAsync(cancellationToken);
        
        // If we get here, the service didn't hang
        await Assert.That(true).IsTrue();
    }

    [Test]
    [Timeout(30_000)]
    public async Task EventBroadcastService_StartsAndStops_WithoutHanging(CancellationToken cancellationToken)
    {
        // Create a minimal service provider with only the required dependencies for EventBroadcastService
        var services = new ServiceCollection();
        
        // Add logging - this will provide ILogger<T> automatically
        services.AddLogging(builder => builder.AddConsole());
        
        // Add the service under test
        services.AddHostedService<EventBroadcastService>();
        
        // Add minimal dependencies that EventBroadcastService needs
        services.AddSingleton<SessionAggregateManager>();
        services.AddSingleton<WorldService>();
        services.AddSingleton<SessionService>();
        services.AddSingleton<IEventStore, MockEventStore>();
        
        // Add SignalR (required for IHubContext<WorldHub>)
        services.AddSignalR();
        services.AddSingleton(typeof(CommandQueue<IGameEvent>));
        // Add a mock hub context for testing
        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true; // Enable detailed errors for easier debugging
        });
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Get the hosted service
        var hostedService = serviceProvider.GetRequiredService<IHostedService>();
        
        // Start the service
        await hostedService.StartAsync(cancellationToken);
        
        // Wait a bit to see if it hangs
        await Task.Delay(1000, cancellationToken);
        
        // Stop the service
        await hostedService.StopAsync(cancellationToken);
        
        // If we get here, the service didn't hang
        await Assert.That(true).IsTrue();
    }

    [Test]
    [Timeout(30_000)]
    public async Task AllHostedServices_Together_StartAndStop_WithoutHanging(CancellationToken cancellationToken)
    {
        // Create a minimal service provider with all hosted services
        var services = new ServiceCollection();
        
        // Add db context
        services.AddDbContext<GameDbContext>(options => options.UseSqlite("Data Source=:memory:;Mode=Memory;Cache=Shared"));
        
        // Add logging - this will provide ILogger<T> automatically
        services.AddLogging(builder => builder.AddConsole());
        
        // Add SignalR (required for GameUpdateService)
        services.AddSignalR();
        
        // Add all hosted services
        services.AddHostedService<GameUpdateService>();
        services.AddHostedService<AiTurnService>();
        services.AddHostedService<ProjectionService>();
        services.AddHostedService<EventBroadcastService>();
        
        // Add minimal dependencies that all services need
        services.AddSingleton<SessionAggregateManager>();
        services.AddSingleton<WorldService>();
        services.AddSingleton<SessionService>();
        services.AddSingleton<IEventStore, MockEventStore>();
        services.AddSingleton(typeof(CommandQueue<IGameEvent>));
        services.AddSingleton<IAiStrategy, DefaultAiStrategy>();
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Ensure the database is created
        using (var scope = serviceProvider.CreateScope())
        {
            try
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<GameDbContext>();
                await dbContext.Database.EnsureCreatedAsync(cancellationToken);
            }
            catch (Exception e)
            {
                await Context.Current.OutputWriter.WriteLineAsync($"Error ensuring database creation: {e.Message}");
            }
        }
        
        // Get all hosted services
        var hostedServices = serviceProvider.GetServices<IHostedService>().ToList();
        
        // Start all services
        foreach (var service in hostedServices)
        {
            await service.StartAsync(cancellationToken);
        }
        
        // Wait a bit to see if any hang
        await Task.Delay(2000, cancellationToken);
        
        // Stop all services
        foreach (var service in hostedServices)
        {
            await service.StopAsync(cancellationToken);
        }
        
        // If we get here, no service hung
        await Assert.That(true).IsTrue();
    }
} 