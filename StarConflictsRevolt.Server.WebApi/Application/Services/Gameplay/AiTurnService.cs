using StarConflictsRevolt.Server.WebApi.Core.Domain.Events;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

public class AiTurnService : BackgroundService
{
    private readonly List<Task> _activeOperations = new();
    private readonly SessionAggregateManager _aggregateManager;
    private readonly IAiStrategy _aiStrategy;
    private readonly CommandQueue<IGameEvent> _commandQueue;
    private readonly IEventStore _eventStore;
    private readonly ILogger<AiTurnService> _logger;
    private readonly SemaphoreSlim _operationSemaphore = new(1, 1);

    public AiTurnService(
        ILogger<AiTurnService> logger,
        IEventStore eventStore,
        SessionAggregateManager aggregateManager,
        CommandQueue<IGameEvent> commandQueue,
        IAiStrategy aiStrategy)
    {
        _logger = logger;
        _eventStore = eventStore;
        _aggregateManager = aggregateManager;
        _commandQueue = commandQueue;
        _aiStrategy = aiStrategy;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AiTurnService starting...");
        try
        {
            while (!stoppingToken.IsCancellationRequested)
                try
                {
                    await ProcessAiTurnsWithTimeoutAsync(stoppingToken);
                    await Task.Delay(5000, stoppingToken); // AI turns every 5 seconds
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("AiTurnService cancellation requested.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in AiTurnService main loop");
                    await Task.Delay(5000, stoppingToken);
                }
        }
        finally
        {
            _logger.LogInformation("AiTurnService exiting.");
        }
    }

    private async Task ProcessAiTurnsWithTimeoutAsync(CancellationToken stoppingToken)
    {
        // Create a timeout for this processing cycle
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(30)); // 30-second timeout per cycle

        try
        {
            await _operationSemaphore.WaitAsync(timeoutCts.Token);
            try
            {
                var operationTask = ProcessAiTurnsAsync(timeoutCts.Token);
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
            _logger.LogWarning("AI turn processing cycle timed out");
        }
    }

    private async Task ProcessAiTurnsAsync(CancellationToken cancellationToken)
    {
        var aggregates = _aggregateManager.GetAllAggregates();
        _logger.LogDebug("Processing AI turns for {AggregateCount} aggregates", aggregates.Count());

        foreach (var sessionAggregate in aggregates)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            await ProcessAiTurnForSessionAsync(sessionAggregate, cancellationToken);
        }
    }

    private async Task ProcessAiTurnForSessionAsync(SessionAggregate sessionAggregate, CancellationToken cancellationToken)
    {
        var sessionId = sessionAggregate.SessionId;
        var world = sessionAggregate.World;

        // Only process AI turns for AI players (those with AiStrategy)
        var aiPlayers = world.Players.Where(p => p.AiStrategy != null).ToList();

        if (!aiPlayers.Any())
        {
            _logger.LogDebug("No AI players in session {SessionId}, skipping AI turn", sessionId);
            return;
        }

        _logger.LogDebug("Processing AI turn for {AiPlayerCount} AI players in session {SessionId}", aiPlayers.Count, sessionId);

        foreach (var aiPlayer in aiPlayers)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            await ProcessAiPlayerTurnAsync(sessionAggregate, aiPlayer, cancellationToken);
        }
    }

    private async Task ProcessAiPlayerTurnAsync(SessionAggregate sessionAggregate, PlayerController aiPlayer, CancellationToken cancellationToken)
    {
        var sessionId = sessionAggregate.SessionId;
        var playerId = aiPlayer.PlayerId;

        try
        {
            _logger.LogDebug("Processing AI turn for player {PlayerId} in session {SessionId}", playerId, sessionId);

            // Generate AI commands using the player's AI strategy
            var commands = aiPlayer.GenerateCommands(sessionAggregate.World);

            if (commands.Any())
            {
                _logger.LogInformation("AI player {PlayerId} generated {CommandCount} commands in session {SessionId}",
                    playerId, commands.Count, sessionId);

                foreach (var command in commands)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    _commandQueue.Enqueue(sessionId, command);
                    _logger.LogDebug("Queued AI command {CommandType} for player {PlayerId} in session {SessionId}",
                        command.GetType().Name, playerId, sessionId);
                }
            }
            else
            {
                _logger.LogDebug("AI player {PlayerId} generated no commands in session {SessionId}", playerId, sessionId);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("AI turn processing cancelled for player {PlayerId} in session {SessionId}", playerId, sessionId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing AI turn for player {PlayerId} in session {SessionId}", playerId, sessionId);
            // Don't throw - continue with other AI players
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("AiTurnService stopping...");

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
        base.Dispose();
    }
}