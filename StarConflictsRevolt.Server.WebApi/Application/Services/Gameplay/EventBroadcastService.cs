using Microsoft.AspNetCore.SignalR;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Events;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

public class EventBroadcastService(IEventStore eventStore, IHubContext<WorldHub> hubContext, ILogger<EventBroadcastService> logger) : BackgroundService
{
    private readonly List<Task> _activeOperations = new();
    private readonly SemaphoreSlim _operationSemaphore = new(1, 1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("EventBroadcastService starting...");
        try
        {
            await eventStore.SubscribeAsync(async envelope => { await ProcessEventWithTimeoutAsync(envelope, stoppingToken); }, stoppingToken);

            // Wait until cancellation is requested, then exit promptly
            var tcs = new TaskCompletionSource();
            using (stoppingToken.Register(() => tcs.SetResult()))
            {
                await tcs.Task;
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("EventBroadcastService cancellation requested.");
        }
        finally
        {
            logger.LogInformation("EventBroadcastService exiting.");
        }
    }

    private async Task ProcessEventWithTimeoutAsync(EventEnvelope envelope, CancellationToken stoppingToken)
    {
        // Create a timeout for this operation
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(30)); // 30-second timeout per event

        try
        {
            await _operationSemaphore.WaitAsync(timeoutCts.Token);
            try
            {
                var operationTask = ProcessEventAsync(envelope, timeoutCts.Token);
                _activeOperations.Add(operationTask);

                await operationTask;

                _activeOperations.Remove(operationTask);
            }
            finally
            {
                _operationSemaphore.Release();
            }
        }
        catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
        {
            logger.LogWarning("Event processing timed out for event {EventType} in world {WorldId}",
                envelope.Event.GetType().Name, envelope.WorldId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing event {EventType} in world {WorldId}",
                envelope.Event.GetType().Name, envelope.WorldId);
        }
    }

    private async Task ProcessEventAsync(EventEnvelope envelope, CancellationToken cancellationToken)
    {
        logger.LogInformation("Broadcasting event {EventType} for world {WorldId}", envelope.Event.GetType().Name, envelope.WorldId);

        List<GameObjectUpdate> updates = new();
        switch (envelope.Event)
        {
            case MoveFleetEvent move:
                updates.Add(GameObjectUpdate.Update(move.FleetId, new
                {
                    move.FleetId,
                    move.FromPlanetId,
                    move.ToPlanetId,
                    move.PlayerId,
                    EventType = "FleetMoved"
                }));
                break;
            case BuildStructureEvent build:
                updates.Add(GameObjectUpdate.Update(build.PlanetId, new
                {
                    build.PlanetId,
                    build.StructureType,
                    build.PlayerId,
                    EventType = "StructureBuilt"
                }));
                break;
            case AttackEvent attack:
                updates.Add(GameObjectUpdate.Update(attack.AttackerFleetId, new
                {
                    attack.AttackerFleetId,
                    attack.DefenderFleetId,
                    attack.LocationPlanetId,
                    attack.PlayerId,
                    EventType = "CombatResolved"
                }));
                break;
            case DiplomacyEvent diplo:
                updates.Add(GameObjectUpdate.Update(diplo.PlayerId, new
                {
                    diplo.PlayerId,
                    diplo.TargetPlayerId,
                    diplo.ProposalType,
                    diplo.Message,
                    EventType = "DiplomacyEvent"
                }));
                break;
            default:
                logger.LogWarning("Unknown event type: {EventType}", envelope.Event.GetType().Name);
                break;
        }

        if (updates.Count > 0)
            try
            {
                // Broadcast to the world group with timeout
                await hubContext.Clients.All.SendAsync("ReceiveUpdates", updates, cancellationToken);
                logger.LogInformation("Successfully broadcast {UpdateCount} updates for event {EventType} in world {WorldId}",
                    updates.Count, envelope.Event.GetType().Name, envelope.WorldId);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning("SignalR broadcast cancelled for event {EventType} in world {WorldId}",
                    envelope.Event.GetType().Name, envelope.WorldId);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to broadcast updates for event {EventType} in world {WorldId}",
                    envelope.Event.GetType().Name, envelope.WorldId);
                throw;
            }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("EventBroadcastService stopping...");

        // Wait for active operations to complete with timeout
        if (_activeOperations.Count > 0)
            try
            {
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(TimeSpan.FromSeconds(10)); // 10-second timeout for shutdown

                await Task.WhenAll(_activeOperations).WaitAsync(timeoutCts.Token);
                logger.LogInformation("All active operations completed during shutdown");
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning("Some operations did not complete during shutdown timeout");
            }

        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _operationSemaphore?.Dispose();
        base.Dispose();
    }
}