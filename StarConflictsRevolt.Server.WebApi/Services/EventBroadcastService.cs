using Microsoft.AspNetCore.SignalR;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Server.WebApi.Eventing;

namespace StarConflictsRevolt.Server.WebApi.Services;

public class EventBroadcastService : BackgroundService
{
    private readonly IEventStore _eventStore;
    private readonly IHubContext<WorldHub> _hubContext;
    private readonly ILogger<EventBroadcastService> _logger;

    public EventBroadcastService(IEventStore eventStore, IHubContext<WorldHub> hubContext, ILogger<EventBroadcastService> logger)
    {
        _eventStore = eventStore;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _eventStore.SubscribeAsync(async envelope =>
        {
            _logger.LogInformation("Broadcasting event {EventType} for world {WorldId}", envelope.Event.GetType().Name, envelope.WorldId);

            List<GameObjectUpdate> updates = new();
            switch (envelope.Event)
            {
                case MoveFleetEvent move:
                    // TODO: Implement actual update logic
                    updates.Add(GameObjectUpdate.Update(move.FleetId, new { FleetId = move.FleetId, ToPlanetId = move.ToPlanetId }));
                    break;
                case BuildStructureEvent build:
                    updates.Add(GameObjectUpdate.Update(build.PlanetId, new { StructureType = build.StructureType }));
                    break;
                case AttackEvent attack:
                    updates.Add(GameObjectUpdate.Update(attack.AttackerFleetId, new { AttackerFleetId = attack.AttackerFleetId, DefenderFleetId = attack.DefenderFleetId }));
                    break;
                case DiplomacyEvent diplo:
                    updates.Add(GameObjectUpdate.Update(diplo.PlayerId, new { TargetPlayerId = diplo.TargetPlayerId, ProposalType = diplo.ProposalType }));
                    break;
                default:
                    _logger.LogWarning("Unknown event type: {EventType}", envelope.Event.GetType().Name);
                    break;
            }
            if (updates.Count > 0)
            {
                // Broadcast to the world group
                await _hubContext.Clients.All.SendAsync("ReceiveUpdates", updates, cancellationToken: stoppingToken);
            }
        }, stoppingToken);
        await Task.Delay(-1, stoppingToken); // Keep service alive
    }
} 