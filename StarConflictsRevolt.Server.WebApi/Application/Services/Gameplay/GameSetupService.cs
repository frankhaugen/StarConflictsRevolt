using StarConflictsRevolt.Server.WebApi.Core.Domain.Enums;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Fleets;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Players;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Structures;
using StarConflictsRevolt.Server.WebApi.Core.Domain.World;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

public class GameSetupService
{
    private readonly ILogger<GameSetupService> _logger;
    private readonly WorldFactory _worldFactory;

    public GameSetupService(WorldFactory worldFactory, ILogger<GameSetupService> logger)
    {
        _worldFactory = worldFactory;
        _logger = logger;
    }

    public async Task<World> CreateNewGameSession(GameSetup setup)
    {
        _logger.LogInformation("Creating new game session: {SessionName} with {PlayerCount} players",
            setup.SessionName, setup.Players.Count);

        // Validate setup
        ValidateGameSetup(setup);

        // Create world with starting conditions
        var world = _worldFactory.CreateStartingWorld(setup);

        // Assign starting positions to players
        AssignStartingPositions(world, setup);

        // Add starting resources and fleets
        AddStartingResources(world, setup);
        AddStartingFleets(world, setup);
        AddStartingStructures(world, setup);

        _logger.LogInformation("Game session created successfully with {PlanetCount} planets",
            world.Galaxy.StarSystems.Sum(s => s.Planets.Count));

        return world;
    }

    private void ValidateGameSetup(GameSetup setup)
    {
        if (string.IsNullOrWhiteSpace(setup.SessionName))
            throw new ArgumentException("Session name cannot be empty");

        if (setup.Players.Count < 1)
            throw new ArgumentException("At least one player is required");

        if (setup.Players.Count > 8)
            throw new ArgumentException("Maximum 8 players allowed");

        int numPlanets = GetPlanetCountForGalaxySize(setup.GalaxySize);
        if (numPlanets < setup.Players.Count)
            throw new ArgumentException("Galaxy size must be at least equal to player count");

        // Validate player names are unique
        var playerNames = setup.Players.Select(p => p.Name).ToList();
        if (playerNames.Count != playerNames.Distinct().Count())
            throw new ArgumentException("Player names must be unique");
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

    private void AssignStartingPositions(World world, GameSetup setup)
    {
        var planets = world.Galaxy.StarSystems
            .SelectMany(s => s.Planets)
            .Where(p => p.PlanetType != null && p.PlanetType.CanBuildStructures)
            .ToList();

        var random = new Random();
        var playerCount = setup.Players.Count;
        var positions = new List<int>();
        var totalPlanets = planets.Count;

        switch (setup.Mode)
        {
            case GameMode.OneVsOne:
                // Opposite sides
                positions = new List<int> { 0, totalPlanets / 2 };
                break;
            case GameMode.TwoVsTwo:
                // Four corners (spread evenly)
                positions = new List<int> { 0, totalPlanets / 3, 2 * totalPlanets / 3, totalPlanets - 1 };
                break;
            case GameMode.FreeForAll:
                // Evenly distributed
                for (var i = 0; i < playerCount; i++)
                    positions.Add(i * totalPlanets / playerCount);
                break;
            case GameMode.HumanVsAI:
            case GameMode.AIvsAI:
            default:
                // Random or fallback
                positions = Enumerable.Range(0, playerCount).Select(_ => random.Next(totalPlanets)).ToList();
                break;
        }

        // Shuffle planets for randomness
        planets = planets.OrderBy(x => random.Next()).ToList();

        for (var i = 0; i < playerCount && i < planets.Count; i++)
        {
            var player = setup.Players[i];
            var planet = planets[positions[i] % planets.Count];
            player.StartingPlanetId = planet.Id;
            // Note: planet.OwnerId is not set here due to record immutability
            _logger.LogDebug("Assigned player {PlayerName} to planet {PlanetName}", player.Name, planet.Name);
        }
    }

    private void AddStartingResources(World world, GameSetup setup)
    {
        // Note: Resources will be added when planets are created with proper values
        // This is a placeholder for future implementation
        _logger.LogDebug("Starting resources will be configured during world creation");
    }

    private void AddStartingFleets(World world, GameSetup setup)
    {
        // Note: Fleets will be added when planets are created with proper values
        // This is a placeholder for future implementation
        _logger.LogDebug("Starting fleets will be configured during world creation");
    }

    private Fleet CreateStartingFleet(PlayerSetup player, GameMode mode, Guid planetId)
    {
        var ships = new List<Ship>();

        // Add ships based on game mode
        switch (mode)
        {
            case GameMode.OneVsOne:
                // Balanced fleet for 1v1
                ships.Add(new Ship(Guid.NewGuid(), "Fighter", false, 20, 20, 2, 5, 2.0));
                ships.Add(new Ship(Guid.NewGuid(), "Scout", false, 10, 10, 1, 3, 3.0));
                break;

            case GameMode.TwoVsTwo:
                // Larger fleet for team games
                ships.Add(new Ship(Guid.NewGuid(), "Fighter", false, 20, 20, 2, 5, 2.0));
                ships.Add(new Ship(Guid.NewGuid(), "Fighter", false, 20, 20, 2, 5, 2.0));
                ships.Add(new Ship(Guid.NewGuid(), "Scout", false, 10, 10, 1, 3, 3.0));
                break;

            case GameMode.FreeForAll:
                // Smaller fleet for FFA
                ships.Add(new Ship(Guid.NewGuid(), "Fighter", false, 20, 20, 2, 5, 2.0));
                ships.Add(new Ship(Guid.NewGuid(), "Scout", false, 10, 10, 1, 3, 3.0));
                break;

            default:
                // Default fleet
                ships.Add(new Ship(Guid.NewGuid(), "Fighter", false, 20, 20, 2, 5, 2.0));
                break;
        }

        return new Fleet(Guid.NewGuid(), $"{player.Name}'s Fleet", ships, planetId, player.Id);
    }

    private void AddStartingStructures(World world, GameSetup setup)
    {
        // Note: Structures will be added when planets are created with proper values
        // This is a placeholder for future implementation
        _logger.LogDebug("Starting structures will be configured during world creation");
    }

    private List<Structure> CreateStartingStructures(PlayerSetup player, GameMode mode)
    {
        // Note: This is a placeholder for future implementation
        // Structures will be created with proper planet references
        return new List<Structure>();
    }
}