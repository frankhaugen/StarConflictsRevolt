using System.Threading.Channels;
using Raven.Client.Documents;

namespace StarConflictsRevolt.Server.WebApi.Eventing;

public class RavenEventStore : IEventStore
{
    readonly Channel<EventEnvelope> _channel;
    readonly IDocumentStore _store;
    readonly CancellationTokenSource _cts = new();
    readonly ILogger<RavenEventStore> _logger;

    public RavenEventStore(IDocumentStore store, ILogger<RavenEventStore> logger, int capacity = 1000)
    {
        _store = store;
        _logger = logger;
        _channel = Channel.CreateBounded<EventEnvelope>(new BoundedChannelOptions(capacity)
        {
            SingleReader = true, SingleWriter = false
        });

        _ = Task.Run(ProcessLoop);
    }

    public async Task PublishAsync(Guid worldId, IGameEvent @event)
    {
        var env = new EventEnvelope(worldId, @event, DateTime.UtcNow);
        _logger.LogDebug("Publishing event to channel: {EventType} for world {WorldId}", @event.GetType().Name, worldId);
        await _channel.Writer.WriteAsync(env);
        _logger.LogDebug("Event published to channel: {EventType} for world {WorldId}", @event.GetType().Name, worldId);
    }

    readonly List<Func<EventEnvelope, Task>> _subscribers = new();

    public Task SubscribeAsync(Func<EventEnvelope, Task> handler, CancellationToken ct)
    {
        _logger.LogDebug("Registering event store subscriber: {Subscriber}", handler.Method.Name);
        _subscribers.Add(handler);
        _logger.LogDebug("Subscriber registered: {Subscriber}", handler.Method.Name);
        return Task.CompletedTask;
    }

    private async Task ProcessLoop()
    {
        var reader = _channel.Reader;
        _logger.LogDebug("RavenEventStore ProcessLoop started");
        while (await reader.WaitToReadAsync(_cts.Token))
        {
            _logger.LogDebug("Channel has data to read");
            var env = await reader.ReadAsync(_cts.Token);
            _logger.LogDebug("Read event from channel: {EventType} for world {WorldId}", env.Event.GetType().Name, env.WorldId);
            // persist to Raven
            using var session = _store.OpenSession();
            session.Advanced.UseOptimisticConcurrency = true;
            session.Store(env);
            session.SaveChanges();
            _logger.LogDebug("Event persisted to RavenDB: {EventType} for world {WorldId}", env.Event.GetType().Name, env.WorldId);

            // dispatch
            foreach (var sub in _subscribers.ToArray())
            {
                _logger.LogDebug("Dispatching event to subscriber: {Subscriber}", sub.Method.Name);
                await sub(env);
                _logger.LogDebug("Event dispatched to subscriber: {Subscriber}", sub.Method.Name);
            }
        }
        _logger.LogDebug("RavenEventStore ProcessLoop exiting");
    }

    // --- Snapshotting and Replay ---
    public IEnumerable<EventEnvelope> GetEventsForWorld(Guid worldId)
    {
        using var session = _store.OpenSession();
        return session.Query<EventEnvelope>().Where(e => e.WorldId == worldId).OrderBy(e => e.Timestamp).ToList();
    }

    public void SnapshotWorld(Guid worldId, object worldState)
    {
        using var session = _store.OpenSession();
        var snapshotTime = DateTime.UtcNow;
        var snapshotDocId = $"WorldSnapshots/{worldId}/{snapshotTime:yyyyMMddHHmmss}";
        session.Store(worldState, snapshotDocId);
        session.SaveChanges();

        // Event scrubbing/aging: delete all events for this world up to the snapshot time
        var oldEvents = session.Query<EventEnvelope>()
            .Where(e => e.WorldId == worldId && e.Timestamp <= snapshotTime)
            .ToList();
        foreach (var ev in oldEvents)
        {
            session.Delete(ev);
        }
        session.SaveChanges();
    }

    public async ValueTask DisposeAsync()
    {
        _logger.LogDebug("Disposing RavenEventStore");
        _channel.Writer.Complete();
        await _cts.CancelAsync();
        await Task.CompletedTask;
        _logger.LogDebug("RavenEventStore disposed");
    }
}