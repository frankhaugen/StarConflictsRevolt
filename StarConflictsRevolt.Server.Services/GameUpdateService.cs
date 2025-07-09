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
    private readonly CommandQueue<IGameEvent> _commandQueue;
    private readonly IEventStore _eventStore;
    private readonly Dictionary<Guid, int> _eventCounts = new();
    private readonly Dictionary<Guid, World> _previousWorldStates = new();

    public GameUpdateService(
        IHubContext<WorldHub> hubContext, 
        ILogger<GameUpdateService> logger, 
        IEventStore eventStore,
        SessionAggregateManager aggregateManager,
        CommandQueue<IGameEvent> commandQueue)
    {
        _hubContext = hubContext;
        _logger = logger;
        _eventStore = eventStore;
        _aggregateManager = aggregateManager;
        _commandQueue = commandQueue;
    }

    // Add this method to create a new session/world
    public void CreateSession(Guid sessionId, World initialWorld)
    {
        _logger.LogInformation("Creating session {SessionId} with world {WorldId}", sessionId, initialWorld.Id);
        _aggregateManager.GetOrCreateAggregate(sessionId, initialWorld);
        _eventCounts[sessionId] = 0;
        _previousWorldStates[sessionId] = initialWorld;
        
        // Send initial world state to clients
        _ = Task.Run(async () =>
        {
            try
            {
                var deltas = ChangeTracker.ComputeDeltas(new World(Guid.Empty, new Galaxy(Guid.Empty, new List<StarSystem>())), initialWorld);
                _logger.LogInformation("Sending initial world state for session {SessionId} with {DeltaCount} updates", sessionId, deltas.Count);
                await _hubContext.Clients.Group(sessionId.ToString()).SendAsync("ReceiveUpdates", deltas);
                _logger.LogInformation("Successfully sent initial world state for session {SessionId}", sessionId);
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
        _logger.LogInformation("GameUpdateService starting...");
        
        // On startup, do not create a demo session. Sessions are created via API.
        // Optionally, load all active sessions from persistent storage here.

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var aggregates = _aggregateManager.GetAllAggregates();
                _logger.LogDebug("Processing {AggregateCount} aggregates", aggregates.Count());
                
                foreach (var sessionAggregate in aggregates)
                {
                    var sessionId = sessionAggregate.SessionId;
                    _logger.LogDebug("Processing session {SessionId}", sessionId);
                    
                    // Process all queued commands for this session
                    var commandsProcessed = 0;
                    while (_commandQueue.TryDequeue(sessionId, out var command))
                    {
                        _logger.LogInformation("Processing command {CommandType} for session {SessionId}", command.GetType().Name, sessionId);
                        
                        var oldWorld = sessionAggregate.World;
                        sessionAggregate.Apply(command);
                        await _eventStore.PublishAsync(sessionId, command);
                        _eventCounts[sessionId] = _eventCounts.GetValueOrDefault(sessionId, 0) + 1;
                        commandsProcessed++;
                        
                        _logger.LogInformation("Applied command {CommandType} to session {SessionId}, event count: {EventCount}", 
                            command.GetType().Name, sessionId, _eventCounts[sessionId]);
                    }
                    
                    if (commandsProcessed > 0)
                    {
                        _logger.LogInformation("Processed {CommandCount} commands for session {SessionId}", commandsProcessed, sessionId);
                    }

                    // Compute deltas by comparing with previous state
                    if (_previousWorldStates.TryGetValue(sessionId, out var previousWorld))
                    {
                        var deltas = ChangeTracker.ComputeDeltas(previousWorld, sessionAggregate.World);
                        _logger.LogInformation("Computed {DeltaCount} deltas for session {SessionId}", deltas.Count, sessionId);
                        
                        if (deltas.Count > 0)
                        {
                            try
                            {
                                _logger.LogInformation("Sending {DeltaCount} deltas to session {SessionId} group", deltas.Count, sessionId);
                                await _hubContext.Clients.Group(sessionId.ToString()).SendAsync("ReceiveUpdates", deltas, stoppingToken);
                                _logger.LogInformation("Successfully sent {DeltaCount} deltas to session {SessionId}", deltas.Count, sessionId);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error sending deltas to session {SessionId}", sessionId);
                            }
                        }
                        else
                        {
                            _logger.LogDebug("No deltas to send for session {SessionId}", sessionId);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("No previous world state found for session {SessionId}", sessionId);
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
                            _logger.LogInformation("Created snapshot for session {SessionId} at event {EventCount}", sessionId, _eventCounts[sessionId]);
                        }
                    }
                }
                
                await Task.Delay(100, stoppingToken); // 10 ticks per second for faster processing
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("GameUpdateService stopping due to cancellation");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GameUpdateService main loop");
                await Task.Delay(1000, stoppingToken); // Wait before retrying
            }
        }
        
        _logger.LogInformation("GameUpdateService stopped");
    }
}