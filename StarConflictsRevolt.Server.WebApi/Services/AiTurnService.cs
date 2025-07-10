using StarConflictsRevolt.Server.WebApi.Eventing;
using StarConflictsRevolt.Server.WebApi.Models;

namespace StarConflictsRevolt.Server.WebApi.Services;

public class AiTurnService : BackgroundService
{
    private readonly CommandQueue<IGameEvent> _commandQueue;
    private readonly ILogger<AiTurnService> _logger;
    private readonly SessionAggregateManager _aggregateManager;

    public AiTurnService(
        CommandQueue<IGameEvent> commandQueue, 
        ILogger<AiTurnService> logger, 
        SessionAggregateManager aggregateManager)
    {
        _commandQueue = commandQueue;
        _logger = logger;
        _aggregateManager = aggregateManager;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var aiStrategy = new DefaultAiStrategy();
            foreach (var sessionAggregate in _aggregateManager.GetAllAggregates())
            {
                var sessionId = sessionAggregate.SessionId;
                var aiPlayers = GetAiPlayers(sessionAggregate.World, aiStrategy);
                foreach (var aiPlayer in aiPlayers)
                {
                    try
                    {
                        var commands = aiPlayer.GenerateCommands(sessionAggregate.World);
                        foreach (var command in commands)
                        {
                            _commandQueue.Enqueue(sessionId, command);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error generating AI commands for player {PlayerId} in session {SessionId}", 
                            aiPlayer.PlayerId, sessionId);
                    }
                }
            }
            
            await Task.Delay(2000, stoppingToken); // AI turns every 2 seconds
        }
    }

    // Helper to find AI players in the world (for now, all PlayerControllers with AiStrategy set)
    private List<PlayerController> GetAiPlayers(World world, IAiStrategy aiStrategy)
    {
        // TODO: When World.Players is available, enumerate and assign strategies to all AI players.
        // For now, create a single default AI player per session.
        var aiPlayer = new PlayerController { PlayerId = Guid.NewGuid(), AiStrategy = aiStrategy };
        return new List<PlayerController> { aiPlayer };
    }
} 