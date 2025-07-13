using System.Numerics;
using StarConflictsRevolt.Server.WebApi.Models;

namespace StarConflictsRevolt.Server.WebApi.Services;

public class WorldFactory
{
    public World CreateDefaultWorld()
    {
        // Create a sample world with planets
        var planets = new List<Planet>
        {
            new("Earth", 6371, 5.972e24, 1670, 29.78, 149.6e6, new List<Fleet>(), new List<Structure>()),
            new("Mars", 3389.5, 0.64171e24, 868, 24.077, 227.9e6, new List<Fleet>(), new List<Structure>())
        };

        var systems = new List<StarSystem>
        {
            new(Guid.NewGuid(), "Solar System", planets, new Vector2(0, 0))
        };

        var galaxy = new Galaxy(systems);
        return new World(Guid.NewGuid(), galaxy);
    }
}