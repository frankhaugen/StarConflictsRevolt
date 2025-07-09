using System.Threading.Channels;
using Raven.Client.Documents;

namespace StarConflictsRevolt.Server.Eventing;

public class RavenEventStore : IEventStore
{
    readonly Channel<EventEnvelope> _channel;
    readonly IDocumentStore _store;
    readonly CancellationTokenSource _cts = new();

    public RavenEventStore(IDocumentStore store, int capacity = 1000)
    {
        _store = store;
        _channel = Channel.CreateBounded<EventEnvelope>(new BoundedChannelOptions(capacity)
        {
            SingleReader = true, SingleWriter = false
        });

        _ = Task.Run(ProcessLoop);
    }

    public async Task PublishAsync(Guid worldId, IGameEvent @event)
    {
        var env = new EventEnvelope(worldId, @event, DateTime.UtcNow);
        await _channel.Writer.WriteAsync(env);
    }

    readonly List<Func<EventEnvelope, Task>> _subscribers = new();

    public Task SubscribeAsync(Func<EventEnvelope, Task> handler, CancellationToken ct)
    {
        _subscribers.Add(handler);
        return Task.CompletedTask;
    }

    private async Task ProcessLoop()
    {
        var reader = _channel.Reader;
        while (await reader.WaitToReadAsync(_cts.Token))
        {
            var env = await reader.ReadAsync(_cts.Token);
            // persist to Raven
            using var session = _store.OpenSession();
            session.Advanced.UseOptimisticConcurrency = true;
            session.Store(env);
            session.SaveChanges();

            // dispatch
            foreach (var sub in _subscribers.ToArray())
                await sub(env);
        }
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
        var snapshotDocId = $"WorldSnapshots/{worldId}/{DateTime.UtcNow:yyyyMMddHHmmss}";
        session.Store(worldState, snapshotDocId);
        session.SaveChanges();
    }

    public async ValueTask DisposeAsync()
    {
        _channel.Writer.Complete();
        await _cts.CancelAsync();
        await Task.CompletedTask;
    }
}