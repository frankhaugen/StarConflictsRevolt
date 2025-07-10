using StarConflictsRevolt.Server.WebApi.Eventing;
using StarConflictsRevolt.Server.WebApi.Models;
using StarConflictsRevolt.Server.WebApi.Datastore;
using StarConflictsRevolt.Server.WebApi.Datastore.Extensions;

namespace StarConflictsRevolt.Server.WebApi.Services;

public class AiTurnService : BackgroundService
{
    private readonly CommandQueue<IGameEvent> _commandQueue;
    private readonly ILogger<AiTurnService> _logger;
    private readonly SessionAggregateManager _aggregateManager;
    private readonly GameDbContext _dbContext;

    public AiTurnService(
        CommandQueue<IGameEvent> commandQueue, 
        ILogger<AiTurnService> logger, 
        SessionAggregateManager aggregateManager,
        GameDbContext dbContext)
    {
        _commandQueue = commandQueue;
        _logger = logger;
        _aggregateManager = aggregateManager;
        _dbContext = dbContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var aiStrategy = new DefaultAiStrategy();
            foreach (var sessionAggregate in _aggregateManager.GetAllAggregates())
            {
                var sessionId = sessionAggregate.SessionId;
                
                // Check if this is a single player session
                var session = await _dbContext.GetSessionAsync(sessionId, stoppingToken);
                if (session?.SessionType != SessionType.SinglePlayer)
                {
                    continue; // Skip multiplayer sessions
                }
                
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

    // Helper to find AI players in the world
    private List<PlayerController> GetAiPlayers(World world, IAiStrategy aiStrategy)
    {
        var aiPlayers = new List<PlayerController>();
        
        // If World.Players is available, use it
        if (world.Players != null && world.Players.Any())
        {
            foreach (var player in world.Players)
            {
                // Assign AI strategy to players that don't have one (AI players)
                if (player.AiStrategy == null)
                {
                    player.AiStrategy = aiStrategy;
                    aiPlayers.Add(player);
                    _logger.LogDebug("Assigned AI strategy to player {PlayerId}", player.PlayerId);
                }
            }
        }
        else
        {
            // Fallback: create a single default AI player per session
            var aiPlayer = new PlayerController 
            { 
                PlayerId = Guid.NewGuid(), 
                AiStrategy = aiStrategy,
                Name = $"AI_{Guid.NewGuid():N}"[..8] // Generate a readable AI name
            };
            aiPlayers.Add(aiPlayer);
            _logger.LogDebug("Created fallback AI player {PlayerId} for session", aiPlayer.PlayerId);
        }

        _logger.LogInformation("Found {AiPlayerCount} AI players for session", aiPlayers.Count);
        return aiPlayers;
    }
} 