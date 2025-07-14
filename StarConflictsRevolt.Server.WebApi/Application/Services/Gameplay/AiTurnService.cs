using System.Threading.Channels;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Events;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Enums;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

public class AiTurnService : BackgroundService
{
    private readonly List<Task> _activeOperations = new();
    private readonly SessionAggregateManager _aggregateManager;
    private readonly IAiStrategy _aiStrategy;
    private readonly CommandQueue _commandQueue;
    private readonly IEventStore _eventStore;
    private readonly ILogger<AiTurnService> _logger;
    private readonly SemaphoreSlim _operationSemaphore = new(1, 1);
    private readonly ChannelReader<GameTickMessage> _tickChannelReader;
    private readonly Dictionary<GameSessionId, AiSessionState> _sessionStates = new();
    private readonly object _sessionStatesLock = new();

    // AI action rate limits based on difficulty
    private const int AI_ACTIONS_PER_SECOND_EASY = 1;
    private const int AI_ACTIONS_PER_SECOND_NORMAL = 2;
    private const int AI_ACTIONS_PER_SECOND_HARD = 3;
    private const int AI_ACTIONS_PER_SECOND_EXPERT = 5;

    public AiTurnService(
        ILogger<AiTurnService> logger,
        IEventStore eventStore,
        SessionAggregateManager aggregateManager,
        CommandQueue commandQueue,
        IAiStrategy aiStrategy,
        ChannelReader<GameTickMessage> tickChannelReader)
    {
        _logger = logger;
        _eventStore = eventStore;
        _aggregateManager = aggregateManager;
        _commandQueue = commandQueue;
        _aiStrategy = aiStrategy;
        _tickChannelReader = tickChannelReader;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AiTurnService starting...");
        
        while (await _tickChannelReader.WaitToReadAsync(stoppingToken))
        {
            try
            {
                var tick = await _tickChannelReader.ReadAsync(stoppingToken);
                await ProcessTickAsync(tick, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing tick: {Message}", ex.Message);
            }
        }
    }

    private async Task ProcessTickAsync(GameTickMessage tick, CancellationToken stoppingToken)
    {
        var sessions = _aggregateManager.GetAllAggregates();
        
        foreach (var session in sessions)
        {
            if (stoppingToken.IsCancellationRequested) break;
            
            try
            {
                await ProcessSessionAiTurnAsync(new GameSessionId(session.SessionId), tick, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing AI turn for session {SessionId} on tick {TickNumber}", 
                    session.SessionId, tick.TickNumber);
            }
        }
    }

    private async Task ProcessSessionAiTurnAsync(GameSessionId sessionId, GameTickMessage tick, CancellationToken stoppingToken)
    {
        var sessionState = GetOrCreateSessionState(sessionId);
        var sessionAggregate = _aggregateManager.GetAggregate(sessionId);
        
        if (sessionAggregate == null)
        {
            _logger.LogWarning("Session aggregate not found for AI turn {SessionId}", sessionId);
            return;
        }

        var world = sessionAggregate.World;

        // Only process AI turns for AI players (those with AiStrategy)
        var aiPlayers = world.Players.Where(p => p.AiStrategy != null).ToList();

        if (!aiPlayers.Any())
        {
            return;
        }

        // Check if we should process AI actions based on difficulty and rate limiting
        if (!ShouldProcessAiActions(sessionState, tick.TickNumber))
        {
            return;
        }

        _logger.LogDebug("Processing AI turn for {AiPlayerCount} AI players in session {SessionId} on tick {TickNumber}", 
            aiPlayers.Count, sessionId, tick.TickNumber);

        foreach (var aiPlayer in aiPlayers)
        {
            if (stoppingToken.IsCancellationRequested) break;

            await ProcessAiPlayerTurnAsync(sessionAggregate, aiPlayer, stoppingToken);
        }

        // Update session state
        sessionState.LastAiTick = tick.TickNumber;
        sessionState.LastAiActionTime = tick.Timestamp;
    }

    private bool ShouldProcessAiActions(AiSessionState sessionState, GameTickNumber tickNumber)
    {
        var aiDifficulty = sessionState.AiDifficulty;
        var actionsPerSecond = GetAiActionsPerSecond(aiDifficulty);
        var ticksPerAction = 10 / actionsPerSecond; // 10 ticks per second
        
        // Ensure minimum interval between AI actions
        if (tickNumber - sessionState.LastAiTick < ticksPerAction)
        {
            return false;
        }

        // Add some randomization to make AI behavior less predictable
        var random = new Random((int)(tickNumber + sessionState.SessionId.GetHashCode()));
        var randomFactor = random.NextDouble() * 0.5 + 0.75; // 0.75 to 1.25 multiplier
        
        return random.NextDouble() < (1.0 / ticksPerAction) * randomFactor;
    }

    private int GetAiActionsPerSecond(AiDifficulty difficulty)
    {
        return difficulty switch
        {
            AiDifficulty.Easy => AI_ACTIONS_PER_SECOND_EASY,
            AiDifficulty.Normal => AI_ACTIONS_PER_SECOND_NORMAL,
            AiDifficulty.Hard => AI_ACTIONS_PER_SECOND_HARD,
            AiDifficulty.Expert => AI_ACTIONS_PER_SECOND_EXPERT,
            _ => AI_ACTIONS_PER_SECOND_NORMAL
        };
    }

    private async Task ProcessAiPlayerTurnAsync(SessionAggregate sessionAggregate, PlayerController aiPlayer, CancellationToken stoppingToken)
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
                    if (stoppingToken.IsCancellationRequested) break;

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
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
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

    public void RegisterSession(GameSessionId sessionId, AiDifficulty aiDifficulty = AiDifficulty.Normal)
    {
        lock (_sessionStatesLock)
        {
            _sessionStates[sessionId] = new AiSessionState
            {
                SessionId = sessionId,
                AiDifficulty = aiDifficulty,
                LastAiTick = new GameTickNumber(0),
                LastAiActionTime = new GameTimestamp(DateTime.UtcNow)
            };
        }
        
        _logger.LogInformation("Registered AI session {SessionId} with difficulty {AiDifficulty}", sessionId, aiDifficulty);
    }

    public void UnregisterSession(GameSessionId sessionId)
    {
        lock (_sessionStatesLock)
        {
            _sessionStates.Remove(sessionId);
        }
        
        _logger.LogInformation("Unregistered AI session {SessionId}", sessionId);
    }

    public void UpdateAiDifficulty(GameSessionId sessionId, AiDifficulty newDifficulty)
    {
        lock (_sessionStatesLock)
        {
            if (_sessionStates.TryGetValue(sessionId, out var state))
            {
                state.AiDifficulty = newDifficulty;
                _logger.LogInformation("Updated AI difficulty for session {SessionId} to {AiDifficulty}", sessionId, newDifficulty);
            }
        }
    }

    private AiSessionState GetOrCreateSessionState(GameSessionId sessionId)
    {
        lock (_sessionStatesLock)
        {
            if (!_sessionStates.TryGetValue(sessionId, out var state))
            {
                state = new AiSessionState
                {
                    SessionId = sessionId,
                    AiDifficulty = AiDifficulty.Normal,
                    LastAiTick = new GameTickNumber(0),
                    LastAiActionTime = new GameTimestamp(DateTime.UtcNow)
                };
                _sessionStates[sessionId] = state;
            }
            return state;
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

public class AiSessionState
{
    public GameSessionId SessionId { get; set; }
    public AiDifficulty AiDifficulty { get; set; }
    public GameTickNumber LastAiTick { get; set; }
    public GameTimestamp LastAiActionTime { get; set; }
}