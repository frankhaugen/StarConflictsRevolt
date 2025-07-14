using Microsoft.EntityFrameworkCore;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Gameplay;
using StarConflictsRevolt.Server.WebApi.Infrastructure.Datastore;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

public class LeaderboardService
{
    private readonly GameDbContext _dbContext;
    private readonly ILogger<LeaderboardService> _logger;

    public LeaderboardService(GameDbContext dbContext, ILogger<LeaderboardService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<List<PlayerStats>> GetLeaderboardAsync(Guid sessionId, CancellationToken ct = default)
    {
        return await _dbContext.PlayerStats
            .Where(ps => ps.SessionId == sessionId)
            .OrderByDescending(ps => ps.BattlesWon)
            .ThenByDescending(ps => ps.PlanetsControlled)
            .ThenByDescending(ps => ps.FleetsOwned)
            .ToListAsync(ct);
    }

    public async Task<PlayerStats?> GetPlayerStatsAsync(Guid sessionId, Guid playerId, CancellationToken ct = default)
    {
        return await _dbContext.PlayerStats
            .FirstOrDefaultAsync(ps => ps.SessionId == sessionId && ps.PlayerId == playerId, ct);
    }

    public async Task<List<PlayerStats>> GetTopPlayersAsync(int count = 10, CancellationToken ct = default)
    {
        return await _dbContext.PlayerStats
            .OrderByDescending(ps => ps.BattlesWon)
            .ThenByDescending(ps => ps.PlanetsControlled)
            .Take(count)
            .ToListAsync(ct);
    }
}