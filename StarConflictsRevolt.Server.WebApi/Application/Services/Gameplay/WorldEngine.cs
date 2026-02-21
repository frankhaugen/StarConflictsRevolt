using Microsoft.AspNetCore.SignalR;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Commands;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Events;

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
        var queued = await _commands.DrainAsync(ct);
        if (queued.Count == 0)
            return;

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
                var tickValue = tick.TickNumber.Value;
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
