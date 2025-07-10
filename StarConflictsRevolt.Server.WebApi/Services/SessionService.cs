using StarConflictsRevolt.Server.WebApi.Datastore;
using StarConflictsRevolt.Server.WebApi.Datastore.Extensions;

namespace StarConflictsRevolt.Server.WebApi.Services;

public class SessionService
{
    private readonly GameDbContext _dbContext;

    public SessionService(GameDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> CreateSessionAsync(string sessionName, CancellationToken cancellationToken = default)
    {
        return await _dbContext.CreateSessionAsync(sessionName, cancellationToken);
    }
}