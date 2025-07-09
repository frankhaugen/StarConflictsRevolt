using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StarConflictsRevolt.Server.Core;
using StarConflictsRevolt.Server.Core.Models;
using StarConflictsRevolt.Server.Eventing;
using StarConflictsRevolt.Server.Services;

namespace StarConflictsRevolt.Server.Services;

public class GameUpdateService : BackgroundService
{
    private readonly IHubContext<WorldHub> _hubContext;
    private readonly ILogger<GameUpdateService> _logger;
    private readonly SessionAggregateManager _aggregateManager;
    private readonly CommandQueue<IGameEvent> _commandQueue = new();
    private readonly IEventStore _eventStore;
    private readonly Dictionary<Guid, int> _eventCounts = new();

    public GameUpdateService(
        IHubContext<WorldHub> hubContext, 
        ILogger<GameUpdateService> logger, 
        IEventStore eventStore,
        SessionAggregateManager aggregateManager)
    {
        _hubContext = hubContext;
        _logger = logger;
        _eventStore = eventStore;
        _aggregateManager = aggregateManager;
    }

    // Add this method to create a new session/world
    public void CreateSession(Guid sessionId, World initialWorld)
    {
        _aggregateManager.GetOrCreateAggregate(sessionId, initialWorld);
        _eventCounts[sessionId] = 0;
    }

    public async Task<bool> SessionExistsAsync(Guid worldId)
    {
        return _aggregateManager.HasAggregate(worldId);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // On startup, do not create a demo session. Sessions are created via API.
        // Optionally, load all active sessions from persistent storage here.

        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var sessionAggregate in _aggregateManager.GetAllAggregates())
            {
                var sessionId = sessionAggregate.SessionId;
                
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
                        // Serialize world as snapshot
                        raven.SnapshotWorld(sessionId, sessionAggregate.World);
                    }
                }
            }
            await Task.Delay(1000, stoppingToken); // 1 tick per second
        }
    }
}