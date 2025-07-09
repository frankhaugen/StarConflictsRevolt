using System.Numerics;
using StarConflictsRevolt.Server.Core.Models;
using StarConflictsRevolt.Server.Services;

namespace StarConflictsRevolt.Server.WebApi;

public class WorldService
{
    private readonly SessionAggregateManager _aggregateManager;

    public WorldService(SessionAggregateManager aggregateManager)
    {
        _aggregateManager = aggregateManager;
    }

    public async Task<World> GetWorldAsync(CancellationToken contextRequestAborted)
    {
        // For now, return the first available session's world
        // In a real implementation, this would take a sessionId parameter
        var aggregates = _aggregateManager.GetAllAggregates();
        if (aggregates.Any())
        {
            return aggregates.First().World;
        }

        // If no sessions exist, create a default world
        return CreateDefaultWorld();
    }

    public async Task<World> GetWorldAsync(Guid sessionId, CancellationToken contextRequestAborted)
    {
        if (_aggregateManager.HasAggregate(sessionId))
        {
            return _aggregateManager.GetOrCreateAggregate(sessionId, CreateDefaultWorld()).World;
        }

        return CreateDefaultWorld();
    }

    private static World CreateDefaultWorld()
    {
        // Create a sample world with planets
        var planets = new List<Planet>
        {
            new Planet("Earth", 6371, 5.972e24, 1670, 29.78, 149.6e6, new(), new()),
            new Planet("Mars", 3389.5, 0.64171e24, 868, 24.077, 227.9e6, new(), new()),
        };
        
        var systems = new List<StarSystem>
        {
            new StarSystem(Guid.NewGuid(), "Solar System", planets, new Vector2(0, 0))
        };

        var galaxy = new Galaxy(systems);
        return new World(Guid.NewGuid(), galaxy);
    }
}