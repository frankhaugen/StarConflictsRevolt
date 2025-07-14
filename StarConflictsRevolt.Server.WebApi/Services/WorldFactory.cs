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

    public World CreateStartingWorld(GameSetup setup)
    {
        var random = new Random();
        var planets = new List<Planet>();
        
        // Create planets based on galaxy size
        for (int i = 0; i < setup.GalaxySize; i++)
        {
            var planetName = GeneratePlanetName(i);
            var planetType = GetRandomPlanetType(random);
            
            var planet = new Planet(
                planetName,
                Radius: random.Next(3000, 8000),
                Mass: random.NextDouble() * 1e25,
                RotationSpeed: random.Next(500, 2000),
                OrbitSpeed: random.Next(20, 40),
                DistanceFromSun: random.Next(50, 300) * 1e6,
                new List<Fleet>(),
                new List<Structure>(),
                PlanetType: GetRandomPlanetType(random)
            );
            
            planets.Add(planet);
        }

        // Create star systems
        var systems = new List<StarSystem>();
        var planetsPerSystem = setup.GalaxySize / setup.StarSystemCount;
        
        for (int i = 0; i < setup.StarSystemCount; i++)
        {
            var systemPlanets = planets
                .Skip(i * planetsPerSystem)
                .Take(planetsPerSystem)
                .ToList();
                
            var systemName = GenerateSystemName(i);
            var coordinates = new Vector2(
                random.Next(-1000, 1000),
                random.Next(-1000, 1000)
            );
            
            var system = new StarSystem(Guid.NewGuid(), systemName, systemPlanets, coordinates);
            systems.Add(system);
        }

        var galaxy = new Galaxy(systems);
        var world = new World(Guid.NewGuid(), galaxy);
        
        // Add player controllers
        foreach (var playerSetup in setup.Players)
        {
            var controller = new PlayerController
            {
                PlayerId = playerSetup.Id,
                Name = playerSetup.Name,
                AiStrategy = playerSetup.AiStrategy
            };
            world.Players.Add(controller);
        }
        
        return world;
    }

    private string GeneratePlanetName(int index)
    {
        var planetNames = new[]
        {
            "Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Zeta", "Eta", "Theta",
            "Iota", "Kappa", "Lambda", "Mu", "Nu", "Xi", "Omicron", "Pi",
            "Rho", "Sigma", "Tau", "Upsilon", "Phi", "Chi", "Psi", "Omega"
        };
        
        return index < planetNames.Length ? planetNames[index] : $"Planet-{index + 1}";
    }

    private string GenerateSystemName(int index)
    {
        var systemNames = new[]
        {
            "Sol", "Proxima", "Alpha Centauri", "Sirius", "Vega", "Arcturus",
            "Rigel", "Betelgeuse", "Antares", "Aldebaran", "Fomalhaut", "Deneb"
        };
        
        return index < systemNames.Length ? systemNames[index] : $"System-{index + 1}";
    }

    private PlanetType GetRandomPlanetType(Random random)
    {
        var types = new[] { PlanetType.Terran, PlanetType.Desert, PlanetType.Ice, PlanetType.Asteroid, PlanetType.Ocean };
        return types[random.Next(types.Length)];
    }
}