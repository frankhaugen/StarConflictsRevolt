using StarConflictsRevolt.Server.Domain.Sessions;

namespace StarConflictsRevolt.Server.Application.Services.Gameplay;

public class SessionService
{
    private readonly IGamePersistence _persistence;

    public SessionService(IGamePersistence persistence)
    {
        _persistence = persistence;
    }

    public Task<Guid> CreateSessionAsync(string sessionName, SessionType sessionType, string? playerId = null, CancellationToken cancellationToken = default)
    {
        return _persistence.CreateSessionAsync(sessionName, sessionType, playerId, cancellationToken);
    }
}