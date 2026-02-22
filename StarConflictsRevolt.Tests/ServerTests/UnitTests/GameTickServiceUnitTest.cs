using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StarConflictsRevolt.Server.Simulation.Engine;

namespace StarConflictsRevolt.Tests.ServerTests.UnitTests;

public class GameTickServiceUnitTest
{
    [Test]
    [Timeout(30_000)]
    public async Task GameTickService_ShouldStartAndStopWithoutErrors(CancellationToken cancellationToken)
    {
        var publisher = new FakeTickPublisher();
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddSingleton<ITickPublisher>(publisher);
        services.AddSingleton<GameTickService>();

        var serviceProvider = services.BuildServiceProvider();
        var gameTickService = serviceProvider.GetRequiredService<GameTickService>();

        await gameTickService.StartAsync(cancellationToken);
        await Task.Delay(2000, cancellationToken);
        await gameTickService.StopAsync(cancellationToken);
    }

    [Test]
    [Timeout(30_000)]
    public async Task GameTickService_ShouldPublishTicksToTransport(CancellationToken cancellationToken)
    {
        var publisher = new FakeTickPublisher();
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddSingleton<ITickPublisher>(publisher);
        services.AddSingleton<GameTickService>();

        var serviceProvider = services.BuildServiceProvider();
        var gameTickService = serviceProvider.GetRequiredService<GameTickService>();

        await gameTickService.StartAsync(cancellationToken);
        await Task.Delay(2000, cancellationToken);
        await gameTickService.StopAsync(cancellationToken);

        publisher.PublishCount.Should().BeGreaterThan(0, "at least one tick should have been published to Transport");
    }

    [Test]
    [Timeout(30_000)]
    public async Task GameTickService_ShouldHandleMultipleListeners(CancellationToken cancellationToken)
    {
        var publisher = new FakeTickPublisher();
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddSingleton<ITickPublisher>(publisher);
        services.AddSingleton<GameTickService>();

        var serviceProvider = services.BuildServiceProvider();
        var gameTickService = serviceProvider.GetRequiredService<GameTickService>();

        await gameTickService.StartAsync(cancellationToken);
        await Task.Delay(2000, cancellationToken);
        await gameTickService.StopAsync(cancellationToken);

        publisher.PublishCount.Should().BeGreaterThan(0);
    }

    private sealed class FakeTickPublisher : ITickPublisher
    {
        private long _publishCount;

        public long PublishCount => Interlocked.Read(ref _publishCount);

        public Task PublishTickAsync(GameTickMessage tick, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _publishCount);
            return Task.CompletedTask;
        }
    }
}
