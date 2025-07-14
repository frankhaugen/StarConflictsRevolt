using StarConflictsRevolt.Server.WebApi.Core.Domain.Sessions;
using StarConflictsRevolt.Server.WebApi.Infrastructure.Datastore;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

public class SessionService
{
    private readonly GameDbContext _dbContext;

    public SessionService(GameDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> CreateSessionAsync(string sessionName, SessionType sessionType, CancellationToken cancellationToken = default)
    {
        return await _dbContext.CreateSessionAsync(sessionName, sessionType, cancellationToken);
    }
}