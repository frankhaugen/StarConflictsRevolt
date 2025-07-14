using System.Threading.Channels;
using Microsoft.AspNetCore.SignalR;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Events;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

public class GameUpdateService : BackgroundService
{
    private readonly List<Task> _activeOperations = new();
    private readonly SessionAggregateManager _aggregateManager;
    private readonly CommandQueue<IGameEvent> _commandQueue;
    private readonly IEventStore _eventStore;
    private readonly IHubContext<WorldHub> _hubContext;
    private readonly ILogger<GameUpdateService> _logger;
    private readonly SemaphoreSlim _operationSemaphore = new(1, 1);
    private readonly Channel<Guid> _sessionNotificationChannel;

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
        _sessionNotificationChannel = Channel.CreateUnbounded<Guid>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("GameUpdateService starting...");
        
        // Start the session processing loop
        var sessionProcessingTask = ProcessSessionsAsync(stoppingToken);
        
        try
        {
            // Wait for cancellation
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("GameUpdateService cancellation requested.");
        }
        finally
        {
            _logger.LogInformation("GameUpdateService exiting.");
            await sessionProcessingTask;
        }
    }

    private async Task ProcessSessionsAsync(CancellationToken stoppingToken)
    {
        var reader = _sessionNotificationChannel.Reader;
        
        while (await reader.WaitToReadAsync(stoppingToken))
        {
            try
            {
                var sessionId = await reader.ReadAsync(stoppingToken);
                await ProcessSessionAggregateAsync(sessionId, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing session notification");
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
        while (_commandQueue.TryDequeue(sessionId, out var command))
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            await ProcessCommandAsync(sessionAggregate, command, cancellationToken);
            commandsProcessed++;
        }

        if (commandsProcessed > 0)
            await SendDeltasAsync(sessionAggregate, cancellationToken);
        else
            _logger.LogDebug("No commands processed for session {SessionId}, skipping delta computation", sessionId);

        // Handle snapshots
        await HandleSnapshotAsync(sessionAggregate, cancellationToken);
    }

    public void NotifySessionHasCommands(Guid sessionId)
    {
        try
        {
            _sessionNotificationChannel.Writer.TryWrite(sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify session {SessionId} has commands", sessionId);
        }
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

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("GameUpdateService stopping...");

        // Complete the channel writer
        _sessionNotificationChannel.Writer.Complete();

        // Wait for active operations to complete with timeout
        if (_activeOperations.Count > 0)
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

        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _operationSemaphore?.Dispose();
        _sessionNotificationChannel?.Writer?.Complete();
        base.Dispose();
    }
}