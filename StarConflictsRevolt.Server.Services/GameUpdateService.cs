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
    private readonly Dictionary<Guid, World> _previousWorldStates = new();

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
        _previousWorldStates[sessionId] = initialWorld;
        
        // Send initial world state to clients
        _ = Task.Run(async () =>
        {
            try
            {
                var deltas = ChangeTracker.ComputeDeltas(new World(Guid.Empty, new Galaxy(Guid.Empty, new List<StarSystem>())), initialWorld);
                await _hubContext.Clients.Group(sessionId.ToString()).SendAsync("ReceiveUpdates", deltas);
                _logger.LogInformation("Sent initial world state for session {SessionId} with {DeltaCount} updates", sessionId, deltas.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending initial world state for session {SessionId}", sessionId);
            }
        });
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

                // Compute deltas by comparing with previous state
                if (_previousWorldStates.TryGetValue(sessionId, out var previousWorld))
                {
                    var deltas = ChangeTracker.ComputeDeltas(previousWorld, sessionAggregate.World);
                    if (deltas.Count > 0)
                    {
                        await _hubContext.Clients.Group(sessionId.ToString()).SendAsync("ReceiveUpdates", deltas, stoppingToken);
                        _logger.LogInformation("Sent {DeltaCount} updates for session {SessionId}", deltas.Count, sessionId);
                    }
                }
                
                // Update the previous state
                _previousWorldStates[sessionId] = sessionAggregate.World;

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