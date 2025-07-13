using Microsoft.AspNetCore.SignalR;
using StarConflictsRevolt.Server.WebApi.Eventing;

namespace StarConflictsRevolt.Server.WebApi.Services;

public class GameUpdateService : BackgroundService
{
    private readonly SessionAggregateManager _aggregateManager;
    private readonly CommandQueue<IGameEvent> _commandQueue;
    private readonly IEventStore _eventStore;
    private readonly IHubContext<WorldHub> _hubContext;
    private readonly ILogger<GameUpdateService> _logger;

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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("GameUpdateService starting...");
        try
        {
            while (!stoppingToken.IsCancellationRequested)
                try
                {
                    var aggregates = _aggregateManager.GetAllAggregates();
                    _logger.LogDebug("Processing {AggregateCount} aggregates", aggregates.Count());
                    foreach (var sessionAggregate in aggregates)
                    {
                        var sessionId = sessionAggregate.SessionId;
                        _logger.LogDebug("Processing session {SessionId}", sessionId);
                        var commandsProcessed = 0;
                        while (_commandQueue.TryDequeue(sessionId, out var command))
                        {
                            _logger.LogInformation("Processing command {CommandType} for session {SessionId}", command.GetType().Name, sessionId);
                            var oldWorld = sessionAggregate.World;
                            sessionAggregate.Apply(command);
                            await _eventStore.PublishAsync(sessionId, command);
                            _aggregateManager.IncrementEventCount(sessionId);
                            _logger.LogInformation("Applied command {CommandType} to session {SessionId}, event count: {EventCount}",
                                command.GetType().Name, sessionId, _aggregateManager.GetEventCount(sessionId));
                        }

                        if (commandsProcessed > 0)
                        {
                            var previousWorld = _aggregateManager.GetPreviousWorldState(sessionId);
                            if (previousWorld != null)
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
                                        _aggregateManager.SetPreviousWorldState(sessionId, sessionAggregate.World);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError(ex, "Error sending deltas to session {SessionId}", sessionId);
                                    }
                                }
                                else
                                {
                                    _logger.LogDebug("No deltas to send for session {SessionId} (no changes detected)", sessionId);
                                    _aggregateManager.SetPreviousWorldState(sessionId, sessionAggregate.World);
                                }
                            }
                            else
                            {
                                _logger.LogWarning("No previous world state found for session {SessionId}", sessionId);
                                _aggregateManager.SetPreviousWorldState(sessionId, sessionAggregate.World);
                            }
                        }
                        else
                        {
                            _logger.LogDebug("No commands processed for session {SessionId}, skipping delta computation", sessionId);
                        }

                        if (_aggregateManager.GetEventCount(sessionId) > 0 && _aggregateManager.GetEventCount(sessionId) % 100 == 0)
                            if (_eventStore is RavenEventStore raven)
                            {
                                raven.SnapshotWorld(sessionId, sessionAggregate.World);
                                _logger.LogInformation("Created snapshot for session {SessionId} at event {EventCount}", sessionId, _aggregateManager.GetEventCount(sessionId));
                            }
                    }

                    await Task.Delay(1000, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("GameUpdateService cancellation requested.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in GameUpdateService main loop");
                    await Task.Delay(1000, stoppingToken);
                }
        }
        finally
        {
            _logger.LogInformation("GameUpdateService exiting.");
        }
    }
}