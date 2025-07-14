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
        services.AddChannel<GameTick>();
        
        services.AddSingleton<GameTickService>();
        
        var serviceProvider = services.BuildServiceProvider();
        var gameTickService = serviceProvider.GetRequiredService<GameTickService>();
        var tickChannelReader = serviceProvider.GetRequiredService<ChannelReader<GameTick>>();
        
        // Start the service
        await gameTickService.StartAsync(cancellationToken);
        
        var receivedTicks = new List<GameTick>();
        var tickReceived = new TaskCompletionSource<GameTick>();

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
        var firstTick = await tickReceived.Task.WaitAsync(TimeSpan.FromSeconds(2));

        // Assert: Should receive ticks
        await Assert.That(firstTick).IsNotNull();
        await Assert.That(firstTick.TickNumber).IsEqualTo(1);
        await Assert.That(firstTick.Timestamp).IsGreaterThan(DateTime.UtcNow.AddSeconds(-1));

        // Wait a bit more and check tick rate
        await Task.Delay(1500); // Wait 1.5 seconds

        // Should have received approximately 15 ticks (10 per second)
        await Assert.That(receivedTicks.Count).IsGreaterThan(10);
        await Assert.That(receivedTicks.Count).IsLessThan(20);

        // Verify tick numbers are sequential
        for (int i = 1; i < receivedTicks.Count; i++)
        {
            await Assert.That(receivedTicks[i].TickNumber).IsEqualTo(receivedTicks[i - 1].TickNumber + 1);
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
        services.AddChannel<GameTick>();
        
        services.AddSingleton<GameTickService>();
        
        var serviceProvider = services.BuildServiceProvider();
        var gameTickService = serviceProvider.GetRequiredService<GameTickService>();
        var tickChannelReader = serviceProvider.GetRequiredService<ChannelReader<GameTick>>();
        
        // Start the service
        await gameTickService.StartAsync(cancellationToken);
        
        var ticks1 = new List<GameTick>();
        var ticks2 = new List<GameTick>();

        // Start two subscribers
        var task1 = Task.Run(async () =>
        {
            while (await tickChannelReader.WaitToReadAsync(CancellationToken.None))
            {
                var tick = await tickChannelReader.ReadAsync(CancellationToken.None);
                ticks1.Add(tick);
                if (ticks1.Count >= 5) break;
            }
        });

        var task2 = Task.Run(async () =>
        {
            while (await tickChannelReader.WaitToReadAsync(CancellationToken.None))
            {
                var tick = await tickChannelReader.ReadAsync(CancellationToken.None);
                ticks2.Add(tick);
                if (ticks2.Count >= 5) break;
            }
        });

        // Act: Wait for both subscribers to receive ticks
        await Task.WhenAll(task1, task2).WaitAsync(TimeSpan.FromSeconds(3));

        // Assert: Both subscribers should receive the same ticks
        await Assert.That(ticks1.Count).IsEqualTo(5);
        await Assert.That(ticks2.Count).IsEqualTo(5);

        for (int i = 0; i < 5; i++)
        {
            await Assert.That(ticks1[i].TickNumber).IsEqualTo(ticks2[i].TickNumber);
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
        services.AddChannel<GameTick>();
        
        services.AddSingleton<GameTickService>();
        
        var serviceProvider = services.BuildServiceProvider();
        var gameTickService = serviceProvider.GetRequiredService<GameTickService>();
        var tickChannelReader = serviceProvider.GetRequiredService<ChannelReader<GameTick>>();
        
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
        });

        // Act: Wait for ticks
        await tickTask.WaitAsync(TimeSpan.FromSeconds(2));

        // Assert: Should have received enough ticks
        await Assert.That(tickTimestamps.Count).IsGreaterThanOrEqualTo(11);

        // Calculate intervals between ticks (should be approximately 100ms)
        var intervals = new List<TimeSpan>();
        for (int i = 1; i < tickTimestamps.Count; i++)
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