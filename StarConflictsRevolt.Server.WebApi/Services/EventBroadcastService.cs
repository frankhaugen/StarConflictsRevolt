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
        _logger.LogInformation("EventBroadcastService starting...");
        try
        {
            await _eventStore.SubscribeAsync(async envelope =>
            {
                _logger.LogInformation("Broadcasting event {EventType} for world {WorldId}", envelope.Event.GetType().Name, envelope.WorldId);

                List<GameObjectUpdate> updates = new();
                switch (envelope.Event)
                {
                    case MoveFleetEvent move:
                        updates.Add(GameObjectUpdate.Update(move.FleetId, new { 
                            FleetId = move.FleetId, 
                            FromPlanetId = move.FromPlanetId,
                            ToPlanetId = move.ToPlanetId,
                            PlayerId = move.PlayerId,
                            EventType = "FleetMoved"
                        }));
                        break;
                    case BuildStructureEvent build:
                        updates.Add(GameObjectUpdate.Update(build.PlanetId, new { 
                            PlanetId = build.PlanetId,
                            StructureType = build.StructureType,
                            PlayerId = build.PlayerId,
                            EventType = "StructureBuilt"
                        }));
                        break;
                    case AttackEvent attack:
                        updates.Add(GameObjectUpdate.Update(attack.AttackerFleetId, new { 
                            AttackerFleetId = attack.AttackerFleetId, 
                            DefenderFleetId = attack.DefenderFleetId,
                            LocationPlanetId = attack.LocationPlanetId,
                            PlayerId = attack.PlayerId,
                            EventType = "CombatResolved"
                        }));
                        break;
                    case DiplomacyEvent diplo:
                        updates.Add(GameObjectUpdate.Update(diplo.PlayerId, new { 
                            PlayerId = diplo.PlayerId,
                            TargetPlayerId = diplo.TargetPlayerId, 
                            ProposalType = diplo.ProposalType,
                            Message = diplo.Message,
                            EventType = "DiplomacyEvent"
                        }));
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
            // Wait until cancellation is requested, then exit promptly
            var tcs = new TaskCompletionSource();
            using (stoppingToken.Register(() => tcs.SetResult()))
            {
                await tcs.Task;
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("EventBroadcastService cancellation requested.");
        }
        finally
        {
            _logger.LogInformation("EventBroadcastService exiting.");
        }
    }
} 