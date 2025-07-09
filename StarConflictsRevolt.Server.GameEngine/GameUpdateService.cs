using System.Numerics;
using Microsoft.AspNetCore.SignalR;
using StarConflictsRevolt.Clients.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StarConflictsRevolt.Server.Core;
using StarConflictsRevolt.Server.Eventing;
using System.Collections.Concurrent;

namespace StarConflictsRevolt.Server.GameEngine;

public class GameUpdateService : BackgroundService
{
    private readonly IHubContext<WorldHub> _hubContext;
    private readonly ILogger<GameUpdateService> _logger;
    // Session state and command queues
    private readonly ConcurrentDictionary<Guid, SessionAggregate> _aggregates = new();
    private readonly CommandQueue<IGameEvent> _commandQueue = new();
    private readonly IEventStore _eventStore;
    private readonly Dictionary<Guid, int> _eventCounts = new();

    public GameUpdateService(IHubContext<WorldHub> hubContext, ILogger<GameUpdateService> logger, IEventStore eventStore)
    {
        _hubContext = hubContext;
        _logger = logger;
        _eventStore = eventStore;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // For demo: create a single session/world
        var worldId = Guid.NewGuid();
        var initialWorld = new World(worldId, new Galaxy(Guid.NewGuid(), new List<StarSystem>()));
        var aggregate = new SessionAggregate(worldId, initialWorld);

        // Replay events from event store
        if (_eventStore is StarConflictsRevolt.Server.Eventing.RavenEventStore ravenStore)
        {
            var events = ravenStore.GetEventsForWorld(worldId).Select(e => e.Event);
            aggregate.ReplayEvents(events);
        }
        _aggregates[worldId] = aggregate;
        _eventCounts[worldId] = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var (sessionId, sessionAggregate) in _aggregates)
            {
                // Process all queued commands for this session
                while (_commandQueue.TryDequeue(sessionId, out var command))
                {
                    sessionAggregate.Apply(command);
                    await _eventStore.PublishAsync(sessionId, command);
                    _eventCounts[sessionId] = _eventCounts.GetValueOrDefault(sessionId, 0) + 1;
                }

                // Compute deltas (stub: use ChangeTracker)
                var deltas = ChangeTracker.ComputeDeltas(sessionAggregate.World, sessionAggregate.World);
                await _hubContext.Clients.Group(sessionId.ToString()).SendAsync("ReceiveUpdates", deltas, stoppingToken);

                // Snapshot every 100 events
                if (_eventCounts[sessionId] > 0 && _eventCounts[sessionId] % 100 == 0)
                {
                    if (_eventStore is StarConflictsRevolt.Server.Eventing.RavenEventStore raven)
                    {
                        // Stub: serialize world as snapshot
                        raven.SnapshotWorld(sessionId, sessionAggregate.World);
                    }
                }
            }
            await Task.Delay(1000, stoppingToken); // 1 tick per second
        }
    }
}