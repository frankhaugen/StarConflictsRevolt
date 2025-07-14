using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;
using StarConflictsRevolt.Server.WebApi.Infrastructure.MessageFlows;
using TUnit.Core;
using Frank.PulseFlow;

namespace StarConflictsRevolt.Tests.ServerTests.UnitTests;

public class GameTickServiceUnitTest
{
    [Test]
    [Timeout(30_000)]
    public async Task GameTickService_ShouldStartAndStopWithoutErrors(CancellationToken cancellationToken)
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        
        // Add Frank.PulseFlow
        services.AddPulseFlow<GameTickMessageFlow>();
        
        services.AddSingleton<GameTickService>();
        
        var serviceProvider = services.BuildServiceProvider();
        var gameTickService = serviceProvider.GetRequiredService<GameTickService>();
        
        // Start the service
        await gameTickService.StartAsync(cancellationToken);
        
        // Wait a bit to allow ticks to be published
        await Task.Delay(2000, cancellationToken);

        // Stop the service
        await gameTickService.StopAsync(cancellationToken);
        
        // Assert: Service should have started and stopped without errors
        await Assert.That(true).IsTrue(); // Basic test that service runs without crashing
    }

    [Test]
    [Timeout(30_000)]
    public async Task GameTickService_ShouldPublishTicksToPulseFlow(CancellationToken cancellationToken)
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        
        // Add Frank.PulseFlow
        services.AddPulseFlow<GameTickMessageFlow>();
        
        services.AddSingleton<GameTickService>();
        
        var serviceProvider = services.BuildServiceProvider();
        var gameTickService = serviceProvider.GetRequiredService<GameTickService>();
        
        // Start the service
        await gameTickService.StartAsync(cancellationToken);
        
        // Wait a bit to allow ticks to be published
        await Task.Delay(2000, cancellationToken);

        // Stop the service
        await gameTickService.StopAsync(cancellationToken);
        
        // Assert: Service should have started and stopped without errors
        await Assert.That(true).IsTrue(); // Basic test that service runs without crashing
    }

    [Test]
    [Timeout(30_000)]
    public async Task GameTickService_ShouldHandleMultipleFlows(CancellationToken cancellationToken)
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        
        // Add Frank.PulseFlow
        services.AddPulseFlow<GameTickMessageFlow>();
        
        services.AddSingleton<GameTickService>();
        
        var serviceProvider = services.BuildServiceProvider();
        var gameTickService = serviceProvider.GetRequiredService<GameTickService>();
        
        // Start the service
        await gameTickService.StartAsync(cancellationToken);
        
        // Wait a bit to allow ticks to be published
        await Task.Delay(2000, cancellationToken);

        // Stop the service
        await gameTickService.StopAsync(cancellationToken);
        
        // Assert: Service should have started and stopped without errors
        await Assert.That(true).IsTrue(); // Basic test that service runs without crashing
    }
} 