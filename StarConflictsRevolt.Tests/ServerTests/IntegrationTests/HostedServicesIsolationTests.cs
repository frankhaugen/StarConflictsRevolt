using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StarConflictsRevolt.Server.WebApi.Services;
using StarConflictsRevolt.Server.WebApi.Eventing;
using StarConflictsRevolt.Tests.TestingInfrastructure;

namespace StarConflictsRevolt.Tests.ServerTests.IntegrationTests;

// Mock implementations to avoid RavenDB dependencies
public class MockEventStore : IEventStore
{
    public Task PublishAsync(Guid worldId, IGameEvent gameEvent)
    {
        return Task.CompletedTask;
    }

    public Task SubscribeAsync(Func<EventEnvelope, Task> handler, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
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
    [Timeout(30_000)]
    public async Task AiTurnService_StartsAndStops_WithoutHanging(CancellationToken cancellationToken)
    {
        // Create a minimal service provider with only the required dependencies for AiTurnService
        var services = new ServiceCollection();
        
        // Add logging - this will provide ILogger<T> automatically
        services.AddLogging(builder => builder.AddConsole());
        
        // Add the service under test
        services.AddHostedService<AiTurnService>();
        
        // Add minimal dependencies that AiTurnService needs
        services.AddSingleton<SessionAggregateManager>();
        services.AddSingleton<WorldService>();
        services.AddSingleton<SessionService>();
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
        
        var serviceProvider = services.BuildServiceProvider();
        
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