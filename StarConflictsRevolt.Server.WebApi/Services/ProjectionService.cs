using Microsoft.EntityFrameworkCore;
using StarConflictsRevolt.Server.WebApi.Datastore;
using StarConflictsRevolt.Server.WebApi.Datastore.Entities;
using StarConflictsRevolt.Server.WebApi.Eventing;

namespace StarConflictsRevolt.Server.WebApi.Services;

public class ProjectionService : BackgroundService
{
    private readonly IEventStore _eventStore;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ProjectionService> _logger;

    public ProjectionService(IEventStore eventStore, IServiceProvider serviceProvider, ILogger<ProjectionService> logger)
    {
        _eventStore = eventStore;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ProjectionService starting...");
        try
        {
            await _eventStore.SubscribeAsync(async envelope =>
            {
                try
                {
                    await UpdateProjectionsAsync(envelope, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating projections for event {EventType} in world {WorldId}", 
                        envelope.Event.GetType().Name, envelope.WorldId);
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
            _logger.LogInformation("ProjectionService cancellation requested.");
        }
        finally
        {
            _logger.LogInformation("ProjectionService exiting.");
        }
    }

    private async Task UpdateProjectionsAsync(EventEnvelope envelope, CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GameDbContext>();

        switch (envelope.Event)
        {
            case MoveFleetEvent move:
                await UpdateFleetOwnershipAsync(dbContext, move, envelope.WorldId, ct);
                break;
            case BuildStructureEvent build:
                await UpdateStructureCountAsync(dbContext, build, envelope.WorldId, ct);
                break;
            case AttackEvent attack:
                await UpdateBattleStatsAsync(dbContext, attack, envelope.WorldId, ct);
                break;
        }

        await dbContext.SaveChangesAsync(ct);
    }

    private async Task UpdateFleetOwnershipAsync(GameDbContext dbContext, MoveFleetEvent move, Guid worldId, CancellationToken ct)
    {
        var stats = await GetOrCreatePlayerStatsAsync(dbContext, move.PlayerId, worldId, ct);
        // Update fleet count logic here
        stats.LastUpdated = DateTime.UtcNow;
    }

    private async Task UpdateStructureCountAsync(GameDbContext dbContext, BuildStructureEvent build, Guid worldId, CancellationToken ct)
    {
        var stats = await GetOrCreatePlayerStatsAsync(dbContext, build.PlayerId, worldId, ct);
        stats.StructuresBuilt++;
        stats.LastUpdated = DateTime.UtcNow;
    }

    private async Task UpdateBattleStatsAsync(GameDbContext dbContext, AttackEvent attack, Guid worldId, CancellationToken ct)
    {
        var attackerStats = await GetOrCreatePlayerStatsAsync(dbContext, attack.PlayerId, worldId, ct);
        // Simple logic: attacker wins if defender fleet is removed
        attackerStats.BattlesWon++;
        attackerStats.LastUpdated = DateTime.UtcNow;
    }

    private async Task<PlayerStats> GetOrCreatePlayerStatsAsync(GameDbContext dbContext, Guid playerId, Guid sessionId, CancellationToken ct)
    {
        var stats = await dbContext.PlayerStats
            .FirstOrDefaultAsync(s => s.PlayerId == playerId && s.SessionId == sessionId, ct);

        if (stats == null)
        {
            // Try to get player name from the session aggregate
            var playerName = $"Player_{playerId}"; // Default fallback
            
            // In a full implementation, we would query the session aggregate to get the player name
            // For now, we'll use a more descriptive default name
            playerName = $"Player_{playerId:N}"[..12]; // Use first 12 chars of GUID for readability

            stats = new PlayerStats
            {
                Id = Guid.NewGuid(),
                PlayerId = playerId,
                SessionId = sessionId,
                PlayerName = playerName,
                Created = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };
            dbContext.PlayerStats.Add(stats);
        }

        return stats;
    }
} 