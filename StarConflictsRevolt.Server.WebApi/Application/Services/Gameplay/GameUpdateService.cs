using Microsoft.AspNetCore.SignalR;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Events;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

public class GameUpdateService
{
    private readonly SessionAggregateManager _aggregateManager;
    private readonly CommandQueue _commandQueue;
    private readonly IEventStore _eventStore;
    private readonly IHubContext<WorldHub> _hubContext;
    private readonly ILogger<GameUpdateService> _logger;

    public GameUpdateService(
        IHubContext<WorldHub> hubContext,
        ILogger<GameUpdateService> logger,
        IEventStore eventStore,
        SessionAggregateManager aggregateManager,
        CommandQueue commandQueue)
    {
        _hubContext = hubContext;
        _logger = logger;
        _eventStore = eventStore;
        _aggregateManager = aggregateManager;
        _commandQueue = commandQueue;
    }

    public async Task ProcessTickAsync(GameTickMessage tick, CancellationToken cancellationToken)
    {
        await ProcessAllSessionsAsync(cancellationToken);
    }

    private async Task ProcessAllSessionsAsync(CancellationToken stoppingToken)
    {
        var activeSessions = _aggregateManager.GetActiveSessionIds();
        foreach (var sessionId in activeSessions)
        {
            if (stoppingToken.IsCancellationRequested)
                break;
            try
            {
                await ProcessSessionAggregateAsync(sessionId, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing session {SessionId}", sessionId);
            }
        }
    }

    private async Task ProcessSessionAggregateAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        var sessionAggregate = _aggregateManager.GetAggregate(sessionId);
        if (sessionAggregate == null)
        {
            _logger.LogWarning("Session aggregate not found for {SessionId}", sessionId);
            return;
        }
        _logger.LogDebug("Processing session {SessionId}", sessionId);
        var commandsProcessed = 0;
        while (_commandQueue.TryDequeue(sessionId, out var commandMessage))
        {
            if (cancellationToken.IsCancellationRequested)
                break;
            await ProcessCommandAsync(sessionAggregate, commandMessage.Command, cancellationToken);
            commandsProcessed++;
        }
        if (commandsProcessed > 0)
            await SendDeltasAsync(sessionAggregate, cancellationToken);
        else
            _logger.LogDebug("No commands processed for session {SessionId}, skipping delta computation", sessionId);
        await HandleSnapshotAsync(sessionAggregate, cancellationToken);
    }

    private async Task ProcessCommandAsync(SessionAggregate sessionAggregate, IGameEvent command, CancellationToken cancellationToken)
    {
        var sessionId = sessionAggregate.SessionId;
        _logger.LogInformation("Processing command {CommandType} for session {SessionId}", command.GetType().Name, sessionId);
        try
        {
            var oldWorld = sessionAggregate.World;
            sessionAggregate.Apply(command);
            await _eventStore.PublishAsync(sessionId, command);
            _aggregateManager.IncrementEventCount(sessionId);
            _logger.LogInformation("Applied command {CommandType} to session {SessionId}, event count: {EventCount}", command.GetType().Name, sessionId, _aggregateManager.GetEventCount(sessionId));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Command processing cancelled for session {SessionId}", sessionId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing command {CommandType} for session {SessionId}", command.GetType().Name, sessionId);
            throw;
        }
    }

    private async Task SendDeltasAsync(SessionAggregate sessionAggregate, CancellationToken cancellationToken)
    {
        var sessionId = sessionAggregate.SessionId;
        var previousWorld = _aggregateManager.GetPreviousWorldState(sessionId);
        if (previousWorld != null)
        {
            var deltas = ChangeTracker.ComputeDeltas(previousWorld, sessionAggregate.World);
            _logger.LogInformation("Computed {DeltaCount} deltas for session {SessionId}", deltas.Count, sessionId);
            if (deltas.Count > 0)
                try
                {
                    using var sendTimeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    sendTimeoutCts.CancelAfter(TimeSpan.FromSeconds(10));
                    _logger.LogInformation("Sending {DeltaCount} deltas to session {SessionId} group", deltas.Count, sessionId);
                    await _hubContext.Clients.Group(sessionId.ToString()).SendAsync("ReceiveUpdates", deltas, sendTimeoutCts.Token);
                    _logger.LogInformation("Successfully sent {DeltaCount} deltas to session {SessionId}", deltas.Count, sessionId);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Delta sending cancelled for session {SessionId}", sessionId);
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending deltas to session {SessionId}", sessionId);
                    throw;
                }
            else
                _logger.LogDebug("No deltas to send for session {SessionId} (no changes detected)", sessionId);
            _aggregateManager.SetPreviousWorldState(sessionId, sessionAggregate.World);
        }
        else
        {
            _logger.LogWarning("No previous world state found for session {SessionId}", sessionId);
            _aggregateManager.SetPreviousWorldState(sessionId, sessionAggregate.World);
        }
    }

    private async Task HandleSnapshotAsync(SessionAggregate sessionAggregate, CancellationToken cancellationToken)
    {
        var sessionId = sessionAggregate.SessionId;
        var eventCount = _aggregateManager.GetEventCount(sessionId);
        if (eventCount > 0 && eventCount % 100 == 0)
            if (_eventStore is RavenEventStore raven)
                try
                {
                    using var snapshotTimeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    snapshotTimeoutCts.CancelAfter(TimeSpan.FromSeconds(30));
                    raven.SnapshotWorld(sessionId, sessionAggregate.World);
                    _logger.LogInformation("Created snapshot for session {SessionId} at event {EventCount}", sessionId, eventCount);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Snapshot creation cancelled for session {SessionId}", sessionId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating snapshot for session {SessionId}", sessionId);
                }
    }
}