using System.Numerics;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Enums;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Fleets;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Galaxies;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Planets;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Stars;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Structures;
using StarConflictsRevolt.Server.WebApi.Core.Domain.World;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

public class WorldFactory
{
    public World CreateDefaultWorld()
    {
        // Create a playable default galaxy with several star systems
        var systems = new List<StarSystem>();

        var solPlanets = new List<Planet>
        {
            new("Earth", 6371, 5.972e24, 1670, 29.78, 149.6e6, new List<Fleet>(), new List<Structure>()),
            new("Mars", 3389.5, 0.64171e24, 868, 24.077, 227.9e6, new List<Fleet>(), new List<Structure>())
        };
        systems.Add(new StarSystem(Guid.NewGuid(), "Solar System", solPlanets, new Vector2(0, 0)));

        var proximaPlanets = new List<Planet>
        {
            new("Proxima b", 5000, 1.3e24, 900, 22, 7.5e6, new List<Fleet>(), new List<Structure>()),
            new("Proxima c", 4000, 0.8e24, 600, 18, 15e6, new List<Fleet>(), new List<Structure>())
        };
        systems.Add(new StarSystem(Guid.NewGuid(), "Proxima Centauri", proximaPlanets, new Vector2(120, 80)));

        var alphaPlanets = new List<Planet>
        {
            new("Alpha Prime", 6000, 2e24, 1200, 25, 100e6, new List<Fleet>(), new List<Structure>()),
            new("Alpha Secundus", 3500, 0.6e24, 700, 20, 180e6, new List<Fleet>(), new List<Structure>())
        };
        systems.Add(new StarSystem(Guid.NewGuid(), "Alpha Centauri", alphaPlanets, new Vector2(200, 150)));

        var siriusPlanets = new List<Planet>
        {
            new("Sirius I", 5500, 1.5e24, 1000, 28, 50e6, new List<Fleet>(), new List<Structure>())
        };
        systems.Add(new StarSystem(Guid.NewGuid(), "Sirius", siriusPlanets, new Vector2(280, 50)));

        var galaxy = new Galaxy(systems);
        return new World(Guid.NewGuid(), galaxy);
    }

    public World CreateStartingWorld(GameSetup setup)
    {
        var random = new Random();
        var planets = new List<Planet>();

        int numPlanets = GetPlanetCountForGalaxySize(setup.GalaxySize);

        // Create planets based on galaxy size
        for (var i = 0; i < numPlanets; i++)
        {
            var planetName = GeneratePlanetName(i);
            var planetType = GetRandomPlanetType(random);

            var planet = new Planet(
                planetName,
                random.Next(3000, 8000),
                random.NextDouble() * 1e25,
                random.Next(500, 2000),
                random.Next(20, 40),
                random.Next(50, 300) * 1e6,
                new List<Fleet>(),
                new List<Structure>(),
                PlanetType: GetRandomPlanetType(random)
            );

            planets.Add(planet);
        }

        // Create star systems
        var systems = new List<StarSystem>();
        var planetsPerSystem = numPlanets / setup.StarSystemCount;

        for (var i = 0; i < setup.StarSystemCount; i++)
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

        // Assign starting resources to each player's starting planet
        foreach (var player in setup.Players)
            if (player.StartingPlanetId.HasValue)
            {
                var planet = galaxy.StarSystems.SelectMany(s => s.Planets).FirstOrDefault(p => p.Id == player.StartingPlanetId.Value);
                if (planet != null)
                {
                    // Apply resource bonuses from planet type
                    var pt = planet.PlanetType;
                    var credits = player.StartingCredits + (pt?.CreditsBonus ?? 0);
                    var materials = player.StartingMaterials + (pt?.MaterialsBonus ?? 0);
                    var fuel = player.StartingFuel + (pt?.FuelBonus ?? 0);
                    // Create a new planet record with updated resources
                    var updatedPlanet = planet with { Credits = credits, Materials = materials, Fuel = fuel };
                    // Replace in star system
                    foreach (var system in galaxy.StarSystems)
                        for (var i = 0; i < system.Planets.Count; i++)
                            if (system.Planets[i].Id == updatedPlanet.Id)
                                system.Planets[i] = updatedPlanet;
                }
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

    private int GetPlanetCountForGalaxySize(GalaxySize size)
    {
        return size switch
        {
            GalaxySize.Small => 12,
            GalaxySize.Medium => 24,
            GalaxySize.Large => 36,
            _ => 15
        };
    }
}