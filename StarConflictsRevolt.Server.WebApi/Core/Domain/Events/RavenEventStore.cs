using System.Threading.Channels;
using Raven.Client.Documents;

namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Events;

public class RavenEventStore : IEventStore
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

    public async ValueTask DisposeAsync()
    {
        _logger.LogDebug("Disposing RavenEventStore");
        
        try
        {
            // Complete the channel writer
            _channel.Writer.Complete();
            
            // Cancel the processing loop
            await _cts.CancelAsync();
            
            // Wait for the process loop to complete
            if (!_processLoopTask.IsCompleted)
            {
                await _processLoopTask.WaitAsync(TimeSpan.FromSeconds(10));
            }
            
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
                _logger.LogDebug("Channel has data to read");
                
                try
                {
                    var env = await reader.ReadAsync(_cts.Token);
                    _logger.LogDebug("Read event from channel: {EventType} for world {WorldId}", env.Event.GetType().Name, env.WorldId);
                    
                    // Persist to RavenDB
                    await PersistEventToDatabaseAsync(env);
                    
                    // Dispatch to subscribers
                    await DispatchToSubscribersAsync(env);
                }
                catch (OperationCanceledException) when (_cts.Token.IsCancellationRequested)
                {
                    _logger.LogDebug("ProcessLoop cancelled");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing event in ProcessLoop");
                    // Continue processing other events
                }
            }
        }
        catch (OperationCanceledException) when (_cts.Token.IsCancellationRequested)
        {
            _logger.LogDebug("ProcessLoop cancelled during WaitToReadAsync");
        }
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
                var dispatchTask = subscriber(env);
                dispatchTasks.Add(dispatchTask);
                _logger.LogDebug("Dispatched event to subscriber: {Subscriber}", subscriber.Method.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dispatching event to subscriber: {Subscriber}", subscriber.Method.Name);
            }
        }

        // Wait for all dispatch tasks to complete
        if (dispatchTasks.Count > 0)
        {
            try
            {
                await Task.WhenAll(dispatchTasks);
                _logger.LogDebug("All subscribers processed event: {EventType} for world {WorldId}", env.Event.GetType().Name, env.WorldId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error waiting for subscriber tasks to complete for event {EventType} in world {WorldId}", env.Event.GetType().Name, env.WorldId);
            }
        }
    }

    // --- Snapshotting and Replay ---
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

    public void SnapshotWorld(Guid worldId, object worldState)
    {
        try
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