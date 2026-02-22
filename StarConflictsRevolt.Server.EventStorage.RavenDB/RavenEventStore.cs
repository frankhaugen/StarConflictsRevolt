using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using StarConflictsRevolt.Server.EventStorage.Abstractions;

namespace StarConflictsRevolt.Server.EventStorage.RavenDB;

/// <summary>
/// RavenDB-backed event store: channel for publish, process loop for persist and dispatch.
/// Exposes GetEventsForWorld and SnapshotWorld for replay and snapshotting (RavenDB-specific).
/// </summary>
public sealed class RavenEventStore : IEventStore
{
    private readonly Channel<EventEnvelope> _channel;
    private readonly CancellationTokenSource _cts = new();
    private readonly ILogger<RavenEventStore> _logger;
    private readonly IDocumentStore _store;
    private readonly Task _processLoopTask;
    private readonly List<Func<EventEnvelope, Task>> _subscribers = new();
    private readonly object _subscribersLock = new();

    public RavenEventStore(IDocumentStore store, ILogger<RavenEventStore> logger, int capacity = 1000)
    {
        _store = store;
        _logger = logger;
        _channel = Channel.CreateBounded<EventEnvelope>(new BoundedChannelOptions(capacity)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.Wait
        });

        _processLoopTask = Task.Run(ProcessLoop);
    }

    /// <inheritdoc />
    public async Task PublishAsync(Guid worldId, IGameEvent @event)
    {
        var env = new EventEnvelope(worldId, @event, DateTime.UtcNow);
        _logger.LogDebug("Publishing event to channel: {EventType} for world {WorldId}", @event.GetType().Name, worldId);

        try
        {
            await _channel.Writer.WriteAsync(env, _cts.Token);
            _logger.LogDebug("Event published to channel: {EventType} for world {WorldId}", @event.GetType().Name, worldId);
        }
        catch (OperationCanceledException) when (_cts.Token.IsCancellationRequested)
        {
            _logger.LogWarning("Event publishing cancelled for {EventType} in world {WorldId}", @event.GetType().Name, worldId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event {EventType} to channel for world {WorldId}", @event.GetType().Name, worldId);
            throw;
        }
    }

    /// <inheritdoc />
    public Task SubscribeAsync(Func<EventEnvelope, Task> handler, CancellationToken ct)
    {
        _logger.LogDebug("Registering event store subscriber: {Subscriber}", handler.Method.Name);

        lock (_subscribersLock)
        {
            _subscribers.Add(handler);
        }

        _logger.LogDebug("Subscriber registered: {Subscriber}", handler.Method.Name);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        _logger.LogDebug("Disposing RavenEventStore");

        try
        {
            _channel.Writer.Complete();
            await _cts.CancelAsync();

            if (!_processLoopTask.IsCompleted)
                await _processLoopTask.WaitAsync(TimeSpan.FromSeconds(10));

            _logger.LogDebug("RavenEventStore disposed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during RavenEventStore disposal");
        }
    }

    private async Task ProcessLoop()
    {
        var reader = _channel.Reader;
        _logger.LogDebug("RavenEventStore ProcessLoop started");

        try
        {
            while (await reader.WaitToReadAsync(_cts.Token))
            {
                try
                {
                    var env = await reader.ReadAsync(_cts.Token);
                    _logger.LogDebug("Read event from channel: {EventType} for world {WorldId}", env.Event.GetType().Name, env.WorldId);

                    await PersistEventToDatabaseAsync(env);
                    await DispatchToSubscribersAsync(env);
                }
                catch (OperationCanceledException) when (_cts.Token.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing event in ProcessLoop");
                }
            }
        }
        catch (OperationCanceledException) when (_cts.Token.IsCancellationRequested) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in ProcessLoop");
        }
        finally
        {
            _logger.LogDebug("RavenEventStore ProcessLoop exiting");
        }
    }

    private async Task PersistEventToDatabaseAsync(EventEnvelope env)
    {
        try
        {
            using var session = _store.OpenSession();
            session.Advanced.UseOptimisticConcurrency = true;
            session.Store(env);
            session.SaveChanges();
            _logger.LogDebug("Event persisted to RavenDB: {EventType} for world {WorldId}", env.Event.GetType().Name, env.WorldId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist event {EventType} to RavenDB for world {WorldId}", env.Event.GetType().Name, env.WorldId);
            throw;
        }

        await Task.CompletedTask;
    }

    private async Task DispatchToSubscribersAsync(EventEnvelope env)
    {
        List<Func<EventEnvelope, Task>> subscribersCopy;
        lock (_subscribersLock)
        {
            subscribersCopy = _subscribers.ToList();
        }

        var dispatchTasks = new List<Task>();
        foreach (var subscriber in subscribersCopy)
        {
            try
            {
                dispatchTasks.Add(subscriber(env));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dispatching event to subscriber: {Subscriber}", subscriber.Method.Name);
            }
        }

        if (dispatchTasks.Count > 0)
        {
            try
            {
                await Task.WhenAll(dispatchTasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error waiting for subscriber tasks for event {EventType} in world {WorldId}", env.Event.GetType().Name, env.WorldId);
            }
        }
    }

    /// <summary>
    /// Returns all persisted events for the given world (for replay). RavenDB-specific.
    /// </summary>
    public IEnumerable<EventEnvelope> GetEventsForWorld(Guid worldId)
    {
        try
        {
            using var session = _store.OpenSession();
            return session.Query<EventEnvelope>().Where(e => e.WorldId == worldId).OrderBy(e => e.Timestamp).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get events for world {WorldId}", worldId);
            return Enumerable.Empty<EventEnvelope>();
        }
    }

    /// <summary>
    /// Stores a world snapshot and deletes events for that world up to the snapshot time. RavenDB-specific.
    /// </summary>
    public void SnapshotWorld(Guid worldId, object worldState)
    {
        try
        {
            using var session = _store.OpenSession();
            var snapshotTime = DateTime.UtcNow;
            var snapshotDocId = $"WorldSnapshots/{worldId}/{snapshotTime:yyyyMMddHHmmss}";
            session.Store(worldState, snapshotDocId);
            session.SaveChanges();

            var oldEvents = session.Query<EventEnvelope>()
                .Where(e => e.WorldId == worldId && e.Timestamp <= snapshotTime)
                .ToList();
            foreach (var ev in oldEvents) session.Delete(ev);
            session.SaveChanges();

            _logger.LogInformation("Created snapshot for world {WorldId} and cleaned up {EventCount} old events", worldId, oldEvents.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create snapshot for world {WorldId}", worldId);
            throw;
        }
    }
}
