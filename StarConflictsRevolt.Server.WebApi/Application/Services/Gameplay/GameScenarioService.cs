using StarConflictsRevolt.Server.WebApi.Core.Domain.Enums;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Resources;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Players;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

public interface IGameScenarioService
{
    GameSetup CreateTutorialScenario();
    GameSetup CreateQuickBattleScenario();
    GameSetup CreateEpicBattleScenario();
    GameSetup CreateCustomScenario(GameScenarioConfig config);
    List<GameScenarioConfig> GetAvailableScenarios();
}

public class GameScenarioService : IGameScenarioService
{
    private readonly ILogger<GameScenarioService> _logger;

    public GameScenarioService(ILogger<GameScenarioService> logger)
    {
        _logger = logger;
    }

    public GameSetup CreateTutorialScenario()
    {
        _logger.LogInformation("Creating tutorial scenario");

        var setup = new GameSetup
        {
            Name = "Tutorial - Learn the Basics",
            Description = "A small 1v1 scenario to learn the game mechanics",
            GalaxySize = GalaxySize.Small,
            MaxPlayers = 2,
            GameMode = GameMode.Tutorial,
            StartingResources = new Dictionary<ResourceType, int>
            {
                { ResourceType.Credits, 1000 },
                { ResourceType.Materials, 500 },
                { ResourceType.Fuel, 200 }
            },
            VictoryConditions = new List<VictoryCondition> { VictoryCondition.Military },
            MaxTurns = 50,
            AiDifficulty = AiDifficulty.Easy
        };

        // Add players
        setup.Players.Add(new PlayerSetup
        {
            Type = PlayerType.Human,
            Name = "Player",
            Color = "Blue",
            AiStrategy = null,
            Difficulty = AiDifficulty.Normal
        });

        setup.Players.Add(new PlayerSetup
        {
            Type = PlayerType.AI,
            Name = "Tutorial AI",
            Color = "Red",
            AiStrategy = null, // Will be set by the game setup service
            Difficulty = AiDifficulty.Easy
        });

        return setup;
    }

    public GameSetup CreateQuickBattleScenario()
    {
        _logger.LogInformation("Creating quick battle scenario");

        var setup = new GameSetup
        {
            Name = "Quick Battle - 2v2",
            Description = "A fast-paced 2v2 battle in a small galaxy",
            GalaxySize = GalaxySize.Small,
            MaxPlayers = 4,
            GameMode = GameMode.TeamBattle,
            StartingResources = new Dictionary<ResourceType, int>
            {
                { ResourceType.Credits, 1500 },
                { ResourceType.Materials, 750 },
                { ResourceType.Fuel, 300 }
            },
            VictoryConditions = new List<VictoryCondition> { VictoryCondition.Military, VictoryCondition.Economic },
            MaxTurns = 75,
            AiDifficulty = AiDifficulty.Normal
        };

        // Add players
        setup.Players.Add(new PlayerSetup
        {
            Type = PlayerType.Human,
            Name = "Player 1",
            Color = "Blue",
            AiStrategy = null,
            Difficulty = AiDifficulty.Normal
        });

        setup.Players.Add(new PlayerSetup
        {
            Type = PlayerType.Human,
            Name = "Player 2",
            Color = "Green",
            AiStrategy = null,
            Difficulty = AiDifficulty.Normal
        });

        setup.Players.Add(new PlayerSetup
        {
            Type = PlayerType.AI,
            Name = "AI Commander",
            Color = "Red",
            AiStrategy = null, // Will be set by the game setup service
            Difficulty = AiDifficulty.Normal
        });

        setup.Players.Add(new PlayerSetup
        {
            Type = PlayerType.AI,
            Name = "AI Strategist",
            Color = "Orange",
            AiStrategy = null, // Will be set by the game setup service
            Difficulty = AiDifficulty.Normal
        });

        return setup;
    }

    public GameSetup CreateEpicBattleScenario()
    {
        _logger.LogInformation("Creating epic battle scenario");

        var setup = new GameSetup
        {
            Name = "Epic Battle - 4 Players",
            Description = "A large-scale 4-player battle for experienced commanders",
            GalaxySize = GalaxySize.Large,
            MaxPlayers = 4,
            GameMode = GameMode.FreeForAll,
            StartingResources = new Dictionary<ResourceType, int>
            {
                { ResourceType.Credits, 2000 },
                { ResourceType.Materials, 1000 },
                { ResourceType.Fuel, 500 }
            },
            VictoryConditions = new List<VictoryCondition> 
            { 
                VictoryCondition.Military, 
                VictoryCondition.Economic, 
                VictoryCondition.Technology,
                VictoryCondition.Time 
            },
            MaxTurns = 150,
            AiDifficulty = AiDifficulty.Hard
        };

        // Add players
        setup.Players.Add(new PlayerSetup
        {
            Type = PlayerType.Human,
            Name = "Commander",
            Color = "Blue",
            AiStrategy = null,
            Difficulty = AiDifficulty.Hard
        });

        setup.Players.Add(new PlayerSetup
        {
            Type = PlayerType.AI,
            Name = "Warlord",
            Color = "Red",
            AiStrategy = null, // Will be set by the game setup service
            Difficulty = AiDifficulty.Hard
        });

        setup.Players.Add(new PlayerSetup
        {
            Type = PlayerType.AI,
            Name = "Merchant",
            Color = "Green",
            AiStrategy = null, // Will be set by the game setup service
            Difficulty = AiDifficulty.Hard
        });

        setup.Players.Add(new PlayerSetup
        {
            Type = PlayerType.AI,
            Name = "Defender",
            Color = "Purple",
            AiStrategy = null, // Will be set by the game setup service
            Difficulty = AiDifficulty.Hard
        });

        return setup;
    }

    public GameSetup CreateCustomScenario(GameScenarioConfig config)
    {
        _logger.LogInformation("Creating custom scenario: {ScenarioName}", config.Name);

        var setup = new GameSetup
        {
            Name = config.Name,
            Description = config.Description,
            GalaxySize = config.GalaxySize,
            MaxPlayers = config.MaxPlayers,
            GameMode = config.GameMode,
            StartingResources = config.StartingResources,
            VictoryConditions = config.VictoryConditions,
            MaxTurns = config.MaxTurns,
            AiDifficulty = config.AiDifficulty
        };

        // Add players based on config
        foreach (var playerConfig in config.Players)
        {
            setup.Players.Add(new PlayerSetup
            {
                Type = playerConfig.PlayerType,
                Name = playerConfig.PlayerName,
                Color = playerConfig.PlayerColor,
                AiStrategy = null, // Will be set by the game setup service
                Difficulty = playerConfig.Difficulty ?? AiDifficulty.Normal
            });
        }

        return setup;
    }

    public List<GameScenarioConfig> GetAvailableScenarios()
    {
        return new List<GameScenarioConfig>
        {
            new GameScenarioConfig
            {
                Id = "tutorial",
                Name = "Tutorial - Learn the Basics",
                Description = "A small 1v1 scenario to learn the game mechanics",
                GalaxySize = GalaxySize.Small,
                MaxPlayers = 2,
                GameMode = GameMode.Tutorial,
                AiDifficulty = AiDifficulty.Easy,
                EstimatedDuration = "15-30 minutes"
            },
            new GameScenarioConfig
            {
                Id = "quick-battle",
                Name = "Quick Battle - 2v2",
                Description = "A fast-paced 2v2 battle in a small galaxy",
                GalaxySize = GalaxySize.Small,
                MaxPlayers = 4,
                GameMode = GameMode.TeamBattle,
                AiDifficulty = AiDifficulty.Normal,
                EstimatedDuration = "30-45 minutes"
            },
            new GameScenarioConfig
            {
                Id = "epic-battle",
                Name = "Epic Battle - 4 Players",
                Description = "A large-scale 4-player battle for experienced commanders",
                GalaxySize = GalaxySize.Large,
                MaxPlayers = 4,
                GameMode = GameMode.FreeForAll,
                AiDifficulty = AiDifficulty.Hard,
                EstimatedDuration = "60-90 minutes"
            }
        };
    }
}

public class GameScenarioConfig
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public GalaxySize GalaxySize { get; set; }
    public int MaxPlayers { get; set; }
    public GameMode GameMode { get; set; }
    public Dictionary<ResourceType, int> StartingResources { get; set; } = new();
    public List<VictoryCondition> VictoryConditions { get; set; } = new();
    public int MaxTurns { get; set; }
    public AiDifficulty AiDifficulty { get; set; }
    public string EstimatedDuration { get; set; } = string.Empty;
    public List<PlayerConfig> Players { get; set; } = new();
}

public class PlayerConfig
{
    public PlayerType PlayerType { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public string PlayerColor { get; set; } = string.Empty;
    public AiStrategyType? AiStrategy { get; set; }
    public AiDifficulty? Difficulty { get; set; }
} 