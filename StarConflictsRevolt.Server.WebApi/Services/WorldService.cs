using System.Numerics;
using StarConflictsRevolt.Server.WebApi.Models;

namespace StarConflictsRevolt.Server.WebApi.Services;

public class WorldService
{
    private readonly SessionAggregateManager _aggregateManager;
    private readonly WorldFactory _worldFactory;

    public WorldService(SessionAggregateManager aggregateManager, WorldFactory worldFactory)
    {
        _aggregateManager = aggregateManager;
        _worldFactory = worldFactory;
    }

    public async Task<World> GetWorldAsync(CancellationToken contextRequestAborted)
    {
        var aggregates = _aggregateManager.GetAllAggregates();
        if (aggregates.Any())
        {
            return aggregates.First().World;
        }
        return _worldFactory.CreateDefaultWorld();
    }

    public async Task<World> GetWorldAsync(Guid sessionId, CancellationToken contextRequestAborted)
    {
        if (_aggregateManager.HasAggregate(sessionId))
        {
            return _aggregateManager.GetOrCreateAggregate(sessionId, _worldFactory.CreateDefaultWorld()).World;
        }
        return _worldFactory.CreateDefaultWorld();
    }
}