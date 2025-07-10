using System.Numerics;
using StarConflictsRevolt.Server.WebApi.Models;
using StarConflictsRevolt.Clients.Models;

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

public static class WorldMappingExtensions
{
    public static WorldDto ToDto(this World world)
    {
        return new WorldDto(world.Id, world.Galaxy.ToDto());
    }
    public static GalaxyDto ToDto(this Galaxy galaxy)
    {
        return new GalaxyDto(Guid.NewGuid(), galaxy.StarSystems.Select(s => s.ToDto()));
    }
    public static StarSystemDto ToDto(this StarSystem system)
    {
        return new StarSystemDto(system.Id, system.Name, system.Planets.Select(p => p.ToDto()), system.Coordinates);
    }
    public static PlanetDto ToDto(this Planet planet)
    {
        // Generate a deterministic Guid for planets if not present (for demo, use hash of name)
        var id = (planet.GetType().GetProperty("Id")?.GetValue(planet) as Guid?) ?? Guid.NewGuid();
        return new PlanetDto(id, planet.Name, planet.Radius, planet.Mass, planet.RotationSpeed, planet.OrbitSpeed, planet.DistanceFromSun);
    }
}