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
    private readonly List<Task> _activeOperations = new();
    private readonly SemaphoreSlim _operationSemaphore = new(1, 1);

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
            {
                try
                {
                    await ProcessAggregatesWithTimeoutAsync(stoppingToken);
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
        }
        finally
        {
            _logger.LogInformation("GameUpdateService exiting.");
        }
    }

    private async Task ProcessAggregatesWithTimeoutAsync(CancellationToken stoppingToken)
    {
        // Create a timeout for this processing cycle
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(30)); // 30-second timeout per cycle

        try
        {
            await _operationSemaphore.WaitAsync(timeoutCts.Token);
            try
            {
                var operationTask = ProcessAggregatesAsync(timeoutCts.Token);
                _activeOperations.Add(operationTask);
                
                await operationTask;
                
                _activeOperations.Remove(operationTask);
            }
            finally
            {
                _operationSemaphore.Release();
            }
        }
        catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
        {
            _logger.LogWarning("Aggregate processing cycle timed out");
        }
    }

    private async Task ProcessAggregatesAsync(CancellationToken cancellationToken)
    {
        var aggregates = _aggregateManager.GetAllAggregates();
        _logger.LogDebug("Processing {AggregateCount} aggregates", aggregates.Count());
        
        foreach (var sessionAggregate in aggregates)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            await ProcessSessionAggregateAsync(sessionAggregate, cancellationToken);
        }
    }

    private async Task ProcessSessionAggregateAsync(SessionAggregate sessionAggregate, CancellationToken cancellationToken)
    {
        var sessionId = sessionAggregate.SessionId;
        _logger.LogDebug("Processing session {SessionId}", sessionId);
        
        var commandsProcessed = 0;
        while (_commandQueue.TryDequeue(sessionId, out var command))
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            await ProcessCommandAsync(sessionAggregate, command, cancellationToken);
            commandsProcessed++;
        }

        if (commandsProcessed > 0)
        {
            await SendDeltasAsync(sessionAggregate, cancellationToken);
        }
        else
        {
            _logger.LogDebug("No commands processed for session {SessionId}, skipping delta computation", sessionId);
        }

        // Handle snapshots
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
            
            // Publish event
            await _eventStore.PublishAsync(sessionId, command);
            
            _aggregateManager.IncrementEventCount(sessionId);
            _logger.LogInformation("Applied command {CommandType} to session {SessionId}, event count: {EventCount}",
                command.GetType().Name, sessionId, _aggregateManager.GetEventCount(sessionId));
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
            {
                try
                {
                    // Send deltas with timeout
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
            }
            else
            {
                _logger.LogDebug("No deltas to send for session {SessionId} (no changes detected)", sessionId);
            }
            
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
        {
            if (_eventStore is RavenEventStore raven)
            {
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
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("GameUpdateService stopping...");
        
        // Wait for active operations to complete with timeout
        if (_activeOperations.Count > 0)
        {
            try
            {
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(TimeSpan.FromSeconds(10)); // 10-second timeout for shutdown
                
                await Task.WhenAll(_activeOperations).WaitAsync(timeoutCts.Token);
                _logger.LogInformation("All active operations completed during shutdown");
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Some operations did not complete during shutdown timeout");
            }
        }
        
        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _operationSemaphore?.Dispose();
        base.Dispose();
    }
}