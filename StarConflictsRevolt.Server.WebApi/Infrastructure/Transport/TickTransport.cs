using Microsoft.AspNetCore.SignalR;
using StarConflictsRevolt.Server.Simulation.Engine;
using StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

namespace StarConflictsRevolt.Server.WebApi.Infrastructure.Transport;

/// <summary>
/// Transport implementation: fans out each tick to in-process listeners and SignalR.
/// </summary>
public sealed class TickTransport : ITickPublisher
{
    private readonly IEnumerable<ITickListener> _listeners;
    private readonly IHubContext<WorldHub> _hubContext;
    private readonly SessionAggregateManager _aggregateManager;
    private readonly ILogger<TickTransport> _logger;

    public TickTransport(
        IEnumerable<ITickListener> listeners,
        IHubContext<WorldHub> hubContext,
        SessionAggregateManager aggregateManager,
        ILogger<TickTransport> logger)
    {
        _listeners = listeners;
        _hubContext = hubContext;
        _aggregateManager = aggregateManager;
        _logger = logger;
    }

    public async Task PublishTickAsync(GameTickMessage tick, CancellationToken cancellationToken)
    {
        var tickNumber = tick.TickNumber.Value;

        foreach (var listener in _listeners)
        {
            try
            {
                await listener.OnTickAsync(tick, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tick listener {Listener} failed on tick {TickNumber}", listener.GetType().Name, tickNumber);
                // Continue to other listeners per spec
            }
        }

        await BroadcastTickToSignalRAsync(tickNumber, cancellationToken);
    }

    private async Task BroadcastTickToSignalRAsync(long tickNumber, CancellationToken cancellationToken)
    {
        var activeSessions = _aggregateManager.GetActiveSessionIds();
        foreach (var sessionId in activeSessions)
        {
            if (cancellationToken.IsCancellationRequested) break;
            try
            {
                using var sendTimeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                sendTimeoutCts.CancelAfter(TimeSpan.FromSeconds(5));
                await _hubContext.Clients.Group(sessionId.ToString()).SendAsync("ReceiveTick", tickNumber, sendTimeoutCts.Token);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to send tick {TickNumber} to session {SessionId}", tickNumber, sessionId);
            }
        }
    }
}
