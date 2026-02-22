using System.Numerics;
using StarConflictsRevolt.Server.Domain.Enums;
using StarConflictsRevolt.Server.Domain.Fleets;
using StarConflictsRevolt.Server.Domain.Galaxies;
using StarConflictsRevolt.Server.Domain.Planets;
using StarConflictsRevolt.Server.Domain.Stars;
using StarConflictsRevolt.Server.Domain.Structures;
using StarConflictsRevolt.Server.Domain.World;
using StarConflictsRevolt.Server.AI;

namespace StarConflictsRevolt.Server.Application.Services.Gameplay;

public class WorldFactory
{
    private readonly IAiStrategy? _aiStrategy;

    public WorldFactory(IAiStrategy? aiStrategy = null)
    {
        _aiStrategy = aiStrategy;
    }

    public World CreateDefaultWorld()
    {
        // Create a playable default galaxy: more systems spread across a larger coordinate range (~±800)
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
        systems.Add(new StarSystem(Guid.NewGuid(), "Proxima Centauri", proximaPlanets, new Vector2(280, -220)));

        var alphaPlanets = new List<Planet>
        {
            new("Alpha Prime", 6000, 2e24, 1200, 25, 100e6, new List<Fleet>(), new List<Structure>()),
            new("Alpha Secundus", 3500, 0.6e24, 700, 20, 180e6, new List<Fleet>(), new List<Structure>())
        };
        systems.Add(new StarSystem(Guid.NewGuid(), "Alpha Centauri", alphaPlanets, new Vector2(520, 120)));

        var siriusPlanets = new List<Planet>
        {
            new("Sirius I", 5500, 1.5e24, 1000, 28, 50e6, new List<Fleet>(), new List<Structure>())
        };
        systems.Add(new StarSystem(Guid.NewGuid(), "Sirius", siriusPlanets, new Vector2(-320, 180)));

        var tauCetiPlanets = new List<Planet>
        {
            new("Tau Ceti e", 5500, 1.4e24, 950, 23, 55e6, new List<Fleet>(), new List<Structure>()),
            new("Tau Ceti f", 4200, 0.9e24, 720, 19, 90e6, new List<Fleet>(), new List<Structure>())
        };
        systems.Add(new StarSystem(Guid.NewGuid(), "Tau Ceti", tauCetiPlanets, new Vector2(-580, -260)));

        var epsilonPlanets = new List<Planet>
        {
            new("Epsilon I", 6000, 1.8e24, 1100, 26, 80e6, new List<Fleet>(), new List<Structure>()),
            new("Epsilon II", 3800, 0.7e24, 750, 21, 140e6, new List<Fleet>(), new List<Structure>())
        };
        systems.Add(new StarSystem(Guid.NewGuid(), "Epsilon Eridani", epsilonPlanets, new Vector2(380, -480)));

        var wolfPlanets = new List<Planet>
        {
            new("Wolf 1061c", 4800, 1.2e24, 880, 22, 25e6, new List<Fleet>(), new List<Structure>())
        };
        systems.Add(new StarSystem(Guid.NewGuid(), "Wolf 1061", wolfPlanets, new Vector2(-680, 320)));

        var trappistPlanets = new List<Planet>
        {
            new("TRAPPIST-1d", 3900, 0.3e24, 650, 15, 2e6, new List<Fleet>(), new List<Structure>()),
            new("TRAPPIST-1e", 3700, 0.25e24, 600, 14, 2.8e6, new List<Fleet>(), new List<Structure>())
        };
        systems.Add(new StarSystem(Guid.NewGuid(), "TRAPPIST-1", trappistPlanets, new Vector2(620, 400)));

        var barnardPlanets = new List<Planet>
        {
            new("Barnard b", 4500, 1.1e24, 820, 20, 12e6, new List<Fleet>(), new List<Structure>())
        };
        systems.Add(new StarSystem(Guid.NewGuid(), "Barnard's Star", barnardPlanets, new Vector2(-420, -520)));

        var luytenPlanets = new List<Planet>
        {
            new("Luyten b", 5000, 1.35e24, 910, 23, 18e6, new List<Fleet>(), new List<Structure>())
        };
        systems.Add(new StarSystem(Guid.NewGuid(), "Luyten's Star", luytenPlanets, new Vector2(180, 580)));

        var galaxy = new Galaxy(systems);
        var world = new World(Guid.NewGuid(), galaxy);

        // Add human and AI players so the game has an AI opponent (AI is part of the game)
        var humanId = new Guid("10000000-0000-0000-0000-000000000001");
        var aiId = new Guid("20000000-0000-0000-0000-000000000002");
        world.Players.Add(new PlayerController { PlayerId = humanId, Name = "Human", AiStrategy = null });
        world.Players.Add(new PlayerController { PlayerId = aiId, Name = "AI Commander", AiStrategy = _aiStrategy });

        return world;
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
                AiStrategy = playerSetup.AiStrategy as IAiStrategy
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