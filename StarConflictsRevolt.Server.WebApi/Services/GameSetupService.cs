using StarConflictsRevolt.Server.WebApi.Enums;
using StarConflictsRevolt.Server.WebApi.Models;

namespace StarConflictsRevolt.Server.WebApi.Services;

public class GameSetupService
{
    private readonly WorldFactory _worldFactory;
    private readonly ILogger<GameSetupService> _logger;

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

        if (setup.GalaxySize < setup.Players.Count)
            throw new ArgumentException("Galaxy size must be at least equal to player count");

        // Validate player names are unique
        var playerNames = setup.Players.Select(p => p.Name).ToList();
        if (playerNames.Count != playerNames.Distinct().Count())
            throw new ArgumentException("Player names must be unique");
    }

    private void AssignStartingPositions(World world, GameSetup setup)
    {
        var planets = world.Galaxy.StarSystems
            .SelectMany(s => s.Planets)
            .Where(p => p.PlanetType == PlanetType.Terran) // Start with Terran planets
            .ToList();

        if (planets.Count < setup.Players.Count)
        {
            // If not enough Terran planets, use any available planets
            planets = world.Galaxy.StarSystems
                .SelectMany(s => s.Planets)
                .Where(p => p.PlanetType.CanBuildStructures)
                .ToList();
        }

        // Shuffle planets for random starting positions
        var random = new Random();
        planets = planets.OrderBy(x => random.Next()).ToList();

        for (int i = 0; i < setup.Players.Count && i < planets.Count; i++)
        {
            var player = setup.Players[i];
            var planet = planets[i];
            
            player.StartingPlanetId = planet.Id;
            
            _logger.LogDebug("Assigned player {PlayerName} to planet {PlanetName}", 
                player.Name, planet.Name);
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