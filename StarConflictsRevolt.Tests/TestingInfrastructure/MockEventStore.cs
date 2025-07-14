using System.Collections.Concurrent;
using StarConflictsRevolt.Server.WebApi.Eventing;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

public class MockEventStore : IEventStore
{
    private readonly ConcurrentQueue<EventEnvelope> _events = new();
    private readonly object _lock = new();
    private readonly List<Func<EventEnvelope, Task>> _subscribers = new();
    private bool _disposed;

    public Task PublishAsync(Guid worldId, IGameEvent @event)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(MockEventStore));

        var envelope = new EventEnvelope(worldId, @event, DateTime.UtcNow);

        _events.Enqueue(envelope);

        // Notify all subscribers
        var notifyTasks = new List<Task>();
        lock (_lock)
        {
            foreach (var subscriber in _subscribers) notifyTasks.Add(subscriber(envelope));
        }

        return Task.WhenAll(notifyTasks);
    }

    public Task SubscribeAsync(Func<EventEnvelope, Task> handler, CancellationToken ct)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(MockEventStore));

        lock (_lock)
        {
            _subscribers.Add(handler);
        }

        // Register cancellation to remove the handler
        ct.Register(() =>
        {
            lock (_lock)
            {
                _subscribers.Remove(handler);
            }
        });

        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;

        // Wait a bit for any pending operations to complete
        await Task.Delay(100);

        lock (_lock)
        {
            _subscribers.Clear();
        }

        Clear();
    }

    public List<EventEnvelope> GetAllEvents()
    {
        return _events.ToList();
    }

    public List<EventEnvelope> GetEventsForWorld(Guid worldId)
    {
        return _events.Where(e => e.WorldId == worldId).ToList();
    }

    public void Clear()
    {
        while (_events.TryDequeue(out _))
        {
        }
    }
}