using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StarConflictsRevolt.Server.Application.Services.Gameplay;
using StarConflictsRevolt.Server.AI;
using StarConflictsRevolt.Server.Domain.AI;
using StarConflictsRevolt.Server.EventStorage.Abstractions;
using StarConflictsRevolt.Server.Simulation.Engine;
using StarConflictsRevolt.Server.WebApi.Infrastructure.Datastore.LiteDb;

namespace StarConflictsRevolt.Tests.ServerTests.IntegrationTests;

// Mock implementations to avoid RavenDB dependencies

public class HostedServicesIsolationTests
{
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
        services.AddSingleton<AiMemoryBank>();

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
        // If we get here, the service ran without crashing - no assertion needed
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
        services.AddSingleton<CommandQueue>();
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
        // If we get here, the service ran without crashing - no assertion needed
    }

    [Test]
    [Timeout(30_000)]
    public async Task AllHostedServices_Together_StartAndStop_WithoutHanging(CancellationToken cancellationToken)
    {
        // Create a minimal service provider with all hosted services
        var services = new ServiceCollection();

        // Add LiteDB + IGamePersistence (in-memory)
        var db = new LiteDatabase("Filename=:memory:");
        services.AddSingleton<ILiteDatabase>(db);
        services.AddSingleton<IGamePersistence, LiteDbGamePersistence>();

        // Add logging - this will provide ILogger<T> automatically
        services.AddLogging(builder => builder.AddConsole());

        // Add SignalR (required for GameUpdateService)
        services.AddSignalR();

        // Add all hosted services
        services.AddSingleton<GameUpdateService>();
        services.AddSingleton<AiTurnService>();
        services.AddHostedService<ProjectionService>();
        services.AddHostedService<EventBroadcastService>();

        // Add minimal dependencies that all services need
        services.AddSingleton<SessionAggregateManager>();
        services.AddSingleton<WorldService>();
        services.AddSingleton<SessionService>();
        services.AddSingleton<IEventStore, MockEventStore>();
        services.AddSingleton<CommandQueue>();
        services.AddSingleton<IAiStrategy, DefaultAiStrategy>();
        services.AddSingleton<AiMemoryBank>();

        var serviceProvider = services.BuildServiceProvider();

        // Get all hosted services
        var hostedServices = serviceProvider.GetServices<IHostedService>().ToList();

        // Start all services
        foreach (var service in hostedServices) await service.StartAsync(cancellationToken);

        // Wait a bit to see if any hang
        await Task.Delay(2000, cancellationToken);

        // Stop all services
        foreach (var service in hostedServices) await service.StopAsync(cancellationToken);

        // If we get here, no service hung
        // If we get here, the service ran without crashing - no assertion needed
    }
}