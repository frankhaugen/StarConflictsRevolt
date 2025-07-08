using StarConflictsRevolt.Server.Datastore;
using StarConflictsRevolt.Server.Datastore.Extensions;

namespace StarConflictsRevolt.Server.WebApi;

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