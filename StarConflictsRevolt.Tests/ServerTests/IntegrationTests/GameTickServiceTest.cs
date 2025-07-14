using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Enums;
using StarConflictsRevolt.Tests.TestingInfrastructure;
using TUnit.Core;

namespace StarConflictsRevolt.Tests.ServerTests.IntegrationTests;

public class GameTickServiceTest
{
    [Test]
    [Timeout(30_000)]
    public async Task GameTickService_ShouldPublishTicksAtCorrectRate(CancellationToken cancellationToken)
    {
        // Arrange
        var testHost = new TestHostApplication();
        await testHost.StartServerAsync(cancellationToken);
        
        var gameTickService = testHost.Server.Services.GetRequiredService<GameTickService>();
        var reader = gameTickService.GetTickReader();
        var receivedTicks = new List<GameTick>();
        var tickReceived = new TaskCompletionSource<GameTick>();

        // Subscribe to ticks
        var tickTask = Task.Run(async () =>
        {
            while (await reader.WaitToReadAsync(CancellationToken.None))
            {
                var tick = await reader.ReadAsync(CancellationToken.None);
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
    }

    [Test]
    [Timeout(30_000)]
    public async Task GameTickService_ShouldHandleMultipleSubscribers(CancellationToken cancellationToken)
    {
        // Arrange
        var testHost = new TestHostApplication();
        await testHost.StartServerAsync(cancellationToken);
        
        var gameTickService = testHost.Server.Services.GetRequiredService<GameTickService>();
        var reader1 = gameTickService.GetTickReader();
        var reader2 = gameTickService.GetTickReader();

        var ticks1 = new List<GameTick>();
        var ticks2 = new List<GameTick>();

        // Start two subscribers
        var task1 = Task.Run(async () =>
        {
            while (await reader1.WaitToReadAsync(CancellationToken.None))
            {
                var tick = await reader1.ReadAsync(CancellationToken.None);
                ticks1.Add(tick);
                if (ticks1.Count >= 5) break;
            }
        });

        var task2 = Task.Run(async () =>
        {
            while (await reader2.WaitToReadAsync(CancellationToken.None))
            {
                var tick = await reader2.ReadAsync(CancellationToken.None);
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
    }

    [Test]
    [Timeout(30_000)]
    public async Task GameTickService_ShouldMaintainConsistentTiming(CancellationToken cancellationToken)
    {
        // Arrange
        var testHost = new TestHostApplication();
        await testHost.StartServerAsync(cancellationToken);
        
        var gameTickService = testHost.Server.Services.GetRequiredService<GameTickService>();
        var reader = gameTickService.GetTickReader();
        var tickTimestamps = new List<DateTime>();

        // Subscribe to ticks
        var tickTask = Task.Run(async () =>
        {
            while (await reader.WaitToReadAsync(CancellationToken.None))
            {
                var tick = await reader.ReadAsync(CancellationToken.None);
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
    }
} 