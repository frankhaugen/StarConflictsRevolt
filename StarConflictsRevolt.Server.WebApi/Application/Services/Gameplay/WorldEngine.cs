using Microsoft.AspNetCore.SignalR;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Commands;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Events;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Enums;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Fleets;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Planets;
using WorldState = StarConflictsRevolt.Server.WebApi.Core.Domain.World.World;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

/// <summary>
/// Engine tick: drain commands, run sim per session, apply events, persist, push to clients.
/// </summary>
public sealed class WorldEngine
{
    private readonly ICommandQueue _commands;
    private readonly IGameSim _sim;
    private readonly SessionAggregateManager _aggregateManager;
    private readonly IEventStore _eventStore;
    private readonly IHubContext<WorldHub> _hubContext;
    private readonly ILogger<WorldEngine> _logger;

    public WorldEngine(
        ICommandQueue commands,
        IGameSim sim,
        SessionAggregateManager aggregateManager,
        IEventStore eventStore,
        IHubContext<WorldHub> hubContext,
        ILogger<WorldEngine> logger)
    {
        _commands = commands;
        _sim = sim;
        _aggregateManager = aggregateManager;
        _eventStore = eventStore;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async ValueTask TickAsync(GameTickMessage tick, CancellationToken ct)
    {
        var tickValue = tick.TickNumber.Value;

        // 1. Drain and process commands (move orders, etc.)
        var queued = await _commands.DrainAsync(ct);
        var bySession = queued.GroupBy(q => q.SessionId);
        foreach (var group in bySession)
        {
            if (ct.IsCancellationRequested) break;
            var sessionId = group.Key;
            var sessionAggregate = _aggregateManager.GetAggregate(sessionId.Value);
            if (sessionAggregate == null)
            {
                _logger.LogWarning("Session aggregate not found for {SessionId}", sessionId);
                continue;
            }

            var eventsProcessed = 0;
            foreach (var q in group)
            {
                if (ct.IsCancellationRequested) break;
                var world = sessionAggregate.World;
                var events = _sim.Execute(tickValue, world, q.Command);
                foreach (var evt in events)
                {
                    sessionAggregate.Apply(evt);
                    await _eventStore.PublishAsync(sessionId.Value, evt);
                    _aggregateManager.IncrementEventCount(sessionId.Value);
                    eventsProcessed++;
                }
            }

            if (eventsProcessed > 0)
            {
                await SendDeltasAsync(sessionAggregate, ct);
                await HandleSnapshotAsync(sessionAggregate, ct);
            }
        }

        // 2. Time advancement: every tick, finalize fleet arrivals for all active sessions
        foreach (var sessionId in _aggregateManager.GetActiveSessionIds())
        {
            if (ct.IsCancellationRequested) break;
            var sessionAggregate = _aggregateManager.GetAggregate(sessionId);
            if (sessionAggregate == null) continue;

            var arrivals = CollectFleetArrivals(sessionAggregate.World, tickValue);
            var applied = 0;
            foreach (var evt in arrivals)
            {
                if (ct.IsCancellationRequested) break;
                sessionAggregate.Apply(evt);
                await _eventStore.PublishAsync(sessionId, evt);
                _aggregateManager.IncrementEventCount(sessionId);
                applied++;
            }
            if (applied > 0)
            {
                await SendDeltasAsync(sessionAggregate, ct);
                await HandleSnapshotAsync(sessionAggregate, ct);
            }
        }
    }

    /// <summary>Collect FleetArrived events for fleets that have reached their ETA this tick.</summary>
    private static List<FleetArrived> CollectFleetArrivals(WorldState world, long currentTick)
    {
        var list = new List<FleetArrived>();
        foreach (var system in world.Galaxy.StarSystems)
        {
            foreach (var planet in system.Planets)
            {
                foreach (var fleet in planet.Fleets)
                {
                    if (fleet.Status == FleetStatus.Moving && fleet.EtaTick.HasValue && fleet.EtaTick.Value <= currentTick)
                        list.Add(new FleetArrived(currentTick, fleet.Id, planet.Id));
                }
            }
        }
        return list;
    }

    private async Task SendDeltasAsync(SessionAggregate sessionAggregate, CancellationToken cancellationToken)
    {
        var sessionId = sessionAggregate.SessionId;
        var previousWorld = _aggregateManager.GetPreviousWorldState(sessionId);
        if (previousWorld != null)
        {
            var deltas = ChangeTracker.ComputeDeltas(previousWorld, sessionAggregate.World);
            if (deltas.Count > 0)
            {
                using var sendTimeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                sendTimeoutCts.CancelAfter(TimeSpan.FromSeconds(10));
                await _hubContext.Clients.Group(sessionId.ToString()).SendAsync("ReceiveUpdates", deltas, sendTimeoutCts.Token);
            }
        }
        _aggregateManager.SetPreviousWorldState(sessionId, sessionAggregate.World);
    }

    private async Task HandleSnapshotAsync(SessionAggregate sessionAggregate, CancellationToken cancellationToken)
    {
        var sessionId = sessionAggregate.SessionId;
        var eventCount = _aggregateManager.GetEventCount(sessionId);
        if (eventCount > 0 && eventCount % 100 == 0 && _eventStore is RavenEventStore raven)
        {
            try
            {
                using var snapshotTimeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                snapshotTimeoutCts.CancelAfter(TimeSpan.FromSeconds(30));
                raven.SnapshotWorld(sessionId, sessionAggregate.World);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating snapshot for session {SessionId}", sessionId);
            }
        }
        await Task.CompletedTask;
    }
}
