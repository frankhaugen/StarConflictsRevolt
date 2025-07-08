using System.Numerics;
using StarConflictsRevolt.Server.Core;

namespace StarConflictsRevolt.Server.WebApi;

public class WorldService
{
    public async Task<World> GetWorldAsync(CancellationToken contextRequestAborted)
    {
        // Simulate a delay to mimic an asynchronous operation
        await Task.Delay(1000, contextRequestAborted);

        // Create a sample world with planets
        var planets = new List<Planet>
        {
            new Planet(Guid.NewGuid(), "Earth", 6371, 5.972e24, 1670, 29.78, 149.6e6),
            new Planet(Guid.NewGuid(), "Mars", 3389.5, 0.64171e24, 868, 24.077, 227.9e6)
        };
        
        var systems = new List<StarSystem>
        {
            new StarSystem(Guid.NewGuid(), "Solar System", planets, new Vector2(0, 0))
        };

        var galaxy = new Galaxy(Guid.NewGuid(), systems);
        return new World(Guid.NewGuid(), galaxy);
    }
}