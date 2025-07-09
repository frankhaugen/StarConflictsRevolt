using StarConflictsRevolt.Server.Eventing;

namespace StarConflictsRevolt.Server.GameEngine;

public class AiTurnService : BackgroundService
{
    private readonly IEventStore _eventStore;
    private readonly ILogger<AiTurnService> _logger;
    private readonly Random _random = new();

    public AiTurnService(IEventStore eventStore, ILogger<AiTurnService> logger)
    {
        _eventStore = eventStore;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // TODO: Replace with real AI logic. For now, publish a random event every 10 seconds.
            var aiPlayerId = Guid.NewGuid(); // TODO: Use real AI player IDs
            var worldId = Guid.Empty; // TODO: Use real world/session ID
            var eventType = _random.Next(4);
            IGameEvent aiEvent = eventType switch
            {
                0 => new MoveFleetEvent(aiPlayerId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()),
                1 => new BuildStructureEvent(aiPlayerId, Guid.NewGuid(), "Mine"),
                2 => new AttackEvent(aiPlayerId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()),
                3 => new DiplomacyEvent(aiPlayerId, Guid.NewGuid(), "Alliance", "Let's be friends!"),
                _ => new MoveFleetEvent(aiPlayerId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid())
            };
            _logger.LogInformation("AI publishing event: {EventType}", aiEvent.GetType().Name);
            await _eventStore.PublishAsync(worldId, aiEvent);
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
} 