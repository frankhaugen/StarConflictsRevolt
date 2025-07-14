using StarConflictsRevolt.Server.WebApi.Core.Domain.Events;

namespace StarConflictsRevolt.Tests.ServerTests.IntegrationTests;

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
            if (!cancellationToken.IsCancellationRequested)
                handler(gameEvent).GetAwaiter().GetResult();

        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        // No resources to dispose in this mock
        return ValueTask.CompletedTask;
    }

    public IEnumerable<EventEnvelope> GetEvents(Guid worldId)
    {
        return _events.Where(e => e.WorldId == worldId);
    }

    public Task ClearEventsAsync(Guid worldId)
    {
        _events.RemoveAll(e => e.WorldId == worldId);
        return Task.CompletedTask;
    }
}