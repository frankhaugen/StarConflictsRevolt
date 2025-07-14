using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;
using TUnit.Core;
using Frank.Channels.DependencyInjection;

namespace StarConflictsRevolt.Tests.ServerTests.UnitTests;

public class GameTickServiceUnitTest
{
    [Test]
    [Timeout(30_000)]
    public async Task GameTickService_ShouldPublishTicksAtCorrectRate(CancellationToken cancellationToken)
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        
        // Add Frank.Channels.DependencyInjection
        services.AddChannel<GameTickMessage>();
        
        services.AddSingleton<GameTickService>();
        
        var serviceProvider = services.BuildServiceProvider();
        var gameTickService = serviceProvider.GetRequiredService<GameTickService>();
        var tickChannelReader = serviceProvider.GetRequiredService<ChannelReader<GameTickMessage>>();
        
        // Start the service
        await gameTickService.StartAsync(cancellationToken);
        
        var receivedTicks = new List<GameTickMessage>();
        var tickReceived = new TaskCompletionSource<GameTickMessage>();

        // Subscribe to ticks
        var tickTask = Task.Run(async () =>
        {
            while (await tickChannelReader.WaitToReadAsync(CancellationToken.None))
            {
                var tick = await tickChannelReader.ReadAsync(CancellationToken.None);
                receivedTicks.Add(tick);
                if (receivedTicks.Count == 1)
                {
                    tickReceived.SetResult(tick);
                }
            }
        });

        // Act: Wait for first tick
        var firstTick = await tickReceived.Task.WaitAsync(TimeSpan.FromSeconds(2), cancellationToken);

        // Assert: Should receive ticks
        await Assert.That(firstTick).IsNotNull();
        await Assert.That(firstTick.TickNumber.Value).IsEqualTo(1);
        await Assert.That(firstTick.Timestamp.Value).IsGreaterThan(DateTime.UtcNow.AddSeconds(-1));

        // Wait a bit more and check tick rate
        await Task.Delay(1500, cancellationToken); // Wait 1.5 seconds

        // Should have received approximately 15 ticks (10 per second)
        await Assert.That(receivedTicks.Count).IsGreaterThan(10);
        await Assert.That(receivedTicks.Count).IsLessThan(20);

        // Verify tick numbers are sequential
        for (var i = 1; i < receivedTicks.Count; i++)
        {
            await Assert.That(receivedTicks[i].TickNumber.Value).IsEqualTo(receivedTicks[i - 1].TickNumber.Value + 1);
        }

        // Cleanup
        await gameTickService.StopAsync(cancellationToken);
    }

    [Test]
    [Timeout(30_000)]
    public async Task GameTickService_ShouldHandleMultipleSubscribers(CancellationToken cancellationToken)
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        
        // Add Frank.Channels.DependencyInjection
        services.AddChannel<GameTickMessage>(ChannelType.Unbounded, new ChannelSettings() 
        {
            SingleReader = false, // Allow multiple subscribers
            SingleWriter = true // Only one writer (the GameTickService)
        });
        
        services.AddSingleton<GameTickService>();
        
        var serviceProvider = services.BuildServiceProvider();
        var gameTickService = serviceProvider.GetRequiredService<GameTickService>();
        
        // Start the service
        await gameTickService.StartAsync(cancellationToken);
        
        var ticks1 = new List<GameTickMessage>();
        var ticks2 = new List<GameTickMessage>();

        // Start two subscribers
        var task1 = Task.Run(async () =>
        {
            var tickChannelReader = serviceProvider.GetRequiredService<ChannelReader<GameTickMessage>>();
            while (await tickChannelReader.WaitToReadAsync(cancellationToken))
            {
                var tick = await tickChannelReader.ReadAsync(cancellationToken);
                ticks1.Add(tick);
                if (ticks1.Count >= 5) break;
            }
        }, cancellationToken);

        var task2 = Task.Run(async () =>
        {
            var tickChannelReader = serviceProvider.GetRequiredService<ChannelReader<GameTickMessage>>();
            while (await tickChannelReader.WaitToReadAsync(cancellationToken))
            {
                var tick = await tickChannelReader.ReadAsync(cancellationToken);
                ticks2.Add(tick);
                if (ticks2.Count >= 5) break;
            }
        }, cancellationToken);
        
        // Wait for the service to publish enough ticks
        await Task.Delay(5000, cancellationToken); // Wait for 5 seconds to allow ticks to be published

        // Act: Wait for both subscribers to receive ticks
        Task.WaitAll([task1, task2], TimeSpan.FromSeconds(9).Milliseconds, cancellationToken);

        // Assert: Both subscribers should receive the same ticks
        await Assert.That(ticks1.Count).IsEqualTo(5);
        await Assert.That(ticks2.Count).IsEqualTo(5);

        for (var i = 0; i < 5; i++)
        {
            var tick1 = ticks1[i];
            var tick2 = ticks2[i];
            
            await Context.Current.OutputWriter.WriteLineAsync($"{tick1.TickNumber.Value} - {tick2.TickNumber.Value}");
        }

        // Cleanup
        await gameTickService.StopAsync(cancellationToken);
    }

    [Test]
    [Timeout(30_000)]
    public async Task GameTickService_ShouldMaintainConsistentTiming(CancellationToken cancellationToken)
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        
        // Add Frank.Channels.DependencyInjection
        services.AddChannel<GameTickMessage>();
        
        services.AddSingleton<GameTickService>();
        
        var serviceProvider = services.BuildServiceProvider();
        var gameTickService = serviceProvider.GetRequiredService<GameTickService>();
        var tickChannelReader = serviceProvider.GetRequiredService<ChannelReader<GameTickMessage>>();
        
        // Start the service
        await gameTickService.StartAsync(cancellationToken);
        
        var tickTimestamps = new List<DateTime>();

        // Subscribe to ticks
        var tickTask = Task.Run(async () =>
        {
            while (await tickChannelReader.WaitToReadAsync(CancellationToken.None))
            {
                var tick = await tickChannelReader.ReadAsync(CancellationToken.None);
                tickTimestamps.Add(tick.Timestamp);
                if (tickTimestamps.Count >= 11) break; // Get 11 ticks to measure 10 intervals
            }
        }, cancellationToken);

        // Act: Wait for ticks
        await tickTask.WaitAsync(TimeSpan.FromSeconds(2), cancellationToken);

        // Assert: Should have received enough ticks
        await Assert.That(tickTimestamps.Count).IsGreaterThanOrEqualTo(11);

        // Calculate intervals between ticks (should be approximately 100ms)
        var intervals = new List<TimeSpan>();
        for (var i = 1; i < tickTimestamps.Count; i++)
        {
            intervals.Add(tickTimestamps[i] - tickTimestamps[i - 1]);
        }

        // Check that intervals are reasonable (between 80ms and 120ms to allow for some variance)
        foreach (var interval in intervals)
        {
            await Assert.That(interval.TotalMilliseconds).IsGreaterThan(80);
            await Assert.That(interval.TotalMilliseconds).IsLessThan(120);
        }

        // Average interval should be close to 100ms
        var averageInterval = intervals.Average(i => i.TotalMilliseconds);
        await Assert.That(averageInterval).IsGreaterThan(90);
        await Assert.That(averageInterval).IsLessThan(110);

        // Cleanup
        await gameTickService.StopAsync(cancellationToken);
    }
} 