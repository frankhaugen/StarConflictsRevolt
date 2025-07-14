using StarConflictsRevolt.Server.WebApi.Core.Domain.Events;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

public class ProjectionService(IEventStore eventStore, ILogger<ProjectionService> logger) : BackgroundService
{
    private readonly List<Task> _activeOperations = new();
    private readonly SemaphoreSlim _operationSemaphore = new(1, 1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("ProjectionService starting...");
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
            logger.LogInformation("ProjectionService cancellation requested.");
        }
        finally
        {
            logger.LogInformation("ProjectionService exiting.");
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
        logger.LogInformation("Processing projection for event {EventType} in world {WorldId}",
            envelope.Event.GetType().Name, envelope.WorldId);

        try
        {
            // Process different event types for projections
            switch (envelope.Event)
            {
                case MoveFleetEvent move:
                    await ProcessFleetMoveProjectionAsync(move, cancellationToken);
                    break;
                case BuildStructureEvent build:
                    await ProcessStructureBuildProjectionAsync(build, cancellationToken);
                    break;
                case AttackEvent attack:
                    await ProcessAttackProjectionAsync(attack, cancellationToken);
                    break;
                case DiplomacyEvent diplo:
                    await ProcessDiplomacyProjectionAsync(diplo, cancellationToken);
                    break;
                default:
                    logger.LogDebug("No projection handler for event type {EventType}", envelope.Event.GetType().Name);
                    break;
            }

            logger.LogInformation("Successfully processed projection for event {EventType} in world {WorldId}",
                envelope.Event.GetType().Name, envelope.WorldId);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Projection processing cancelled for event {EventType} in world {WorldId}",
                envelope.Event.GetType().Name, envelope.WorldId);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process projection for event {EventType} in world {WorldId}",
                envelope.Event.GetType().Name, envelope.WorldId);
            throw;
        }
    }

    private async Task ProcessFleetMoveProjectionAsync(MoveFleetEvent move, CancellationToken cancellationToken)
    {
        logger.LogDebug("Processing fleet move projection: Fleet {FleetId} from {FromPlanet} to {ToPlanet}",
            move.FleetId, move.FromPlanetId, move.ToPlanetId);

        // Add projection logic here (e.g., updating leaderboards, statistics, etc.)
        await Task.CompletedTask; // Placeholder for actual projection work
    }

    private async Task ProcessStructureBuildProjectionAsync(BuildStructureEvent build, CancellationToken cancellationToken)
    {
        logger.LogDebug("Processing structure build projection: {StructureType} on planet {PlanetId}",
            build.StructureType, build.PlanetId);

        // Add projection logic here
        await Task.CompletedTask; // Placeholder for actual projection work
    }

    private async Task ProcessAttackProjectionAsync(AttackEvent attack, CancellationToken cancellationToken)
    {
        logger.LogDebug("Processing attack projection: {AttackerFleet} vs {DefenderFleet} at {Location}",
            attack.AttackerFleetId, attack.DefenderFleetId, attack.LocationPlanetId);

        // Add projection logic here
        await Task.CompletedTask; // Placeholder for actual projection work
    }

    private async Task ProcessDiplomacyProjectionAsync(DiplomacyEvent diplo, CancellationToken cancellationToken)
    {
        logger.LogDebug("Processing diplomacy projection: {ProposalType} from {PlayerId} to {TargetPlayerId}",
            diplo.ProposalType, diplo.PlayerId, diplo.TargetPlayerId);

        // Add projection logic here
        await Task.CompletedTask; // Placeholder for actual projection work
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("ProjectionService stopping...");

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