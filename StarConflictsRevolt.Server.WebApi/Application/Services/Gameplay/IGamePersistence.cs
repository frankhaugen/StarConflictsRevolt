using StarConflictsRevolt.Server.WebApi.Core.Domain.Gameplay;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Sessions;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

/// <summary>
///     Persistence for game sessions and clients (replaces SQL/EF for this data).
///     Events remain in RavenDB via IEventStore.
/// </summary>
public interface IGamePersistence
{
    Task<Guid> CreateSessionAsync(string sessionName, SessionType sessionType, string? playerId, CancellationToken cancellationToken = default);
    Task EndSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<Core.Domain.Gameplay.Session?> GetSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<List<Core.Domain.Gameplay.Session>> GetActiveSessionsByPlayerAsync(string playerId, CancellationToken cancellationToken = default);
    Task<List<Core.Domain.Gameplay.Session>> ListActiveSessionsAsync(CancellationToken cancellationToken = default);

    Task<Client?> GetClientAsync(string clientId, CancellationToken cancellationToken = default);
    Task UpsertClientAsync(Client client, CancellationToken cancellationToken = default);

    Task<List<PlayerStats>> GetLeaderboardAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<PlayerStats?> GetPlayerStatsAsync(Guid sessionId, Guid playerId, CancellationToken cancellationToken = default);
    Task<List<PlayerStats>> GetTopPlayersAsync(int count = 10, CancellationToken cancellationToken = default);
}
