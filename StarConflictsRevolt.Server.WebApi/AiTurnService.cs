using StarConflictsRevolt.Server.Core;
using StarConflictsRevolt.Server.Core.Models;
using StarConflictsRevolt.Server.Services;

namespace StarConflictsRevolt.Server.GameEngine;

public class AiTurnService : BackgroundService
{
    private readonly CommandQueue<StarConflictsRevolt.Server.Eventing.IGameEvent> _commandQueue;
    private readonly ILogger<AiTurnService> _logger;
    private readonly SessionAggregateManager _aggregateManager;

    public AiTurnService(
        CommandQueue<StarConflictsRevolt.Server.Eventing.IGameEvent> commandQueue, 
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
            foreach (var sessionAggregate in _aggregateManager.GetAllAggregates())
            {
                var sessionId = sessionAggregate.SessionId;
                
                // Find AI players in this session
                var aiPlayers = GetAiPlayers(sessionAggregate.World);
                
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

    private List<PlayerController> GetAiPlayers(World world)
    {
        // For now, create a simple AI player for demonstration
        // In a real implementation, this would look up actual AI players from the world state
        var aiPlayer = new AiController(new LoggerFactory().CreateLogger<AiController>())
        {
            PlayerId = Guid.NewGuid() // Assign a new GUID for the AI player
        };
        return new List<PlayerController> { aiPlayer };
    }
} 