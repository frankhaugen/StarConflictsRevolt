using StarConflictsRevolt.Server.WebApi.Core.Domain.Gameplay;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

public class LeaderboardService
{
    private readonly IGamePersistence _persistence;
    private readonly ILogger<LeaderboardService> _logger;

    public LeaderboardService(IGamePersistence persistence, ILogger<LeaderboardService> logger)
    {
        _persistence = persistence;
        _logger = logger;
    }

    public Task<List<PlayerStats>> GetLeaderboardAsync(Guid sessionId, CancellationToken ct = default)
        => _persistence.GetLeaderboardAsync(sessionId, ct);

    public Task<PlayerStats?> GetPlayerStatsAsync(Guid sessionId, Guid playerId, CancellationToken ct = default)
        => _persistence.GetPlayerStatsAsync(sessionId, playerId, ct);

    public Task<List<PlayerStats>> GetTopPlayersAsync(int count = 10, CancellationToken ct = default)
        => _persistence.GetTopPlayersAsync(count, ct);
}