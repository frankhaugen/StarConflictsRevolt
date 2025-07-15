using StarConflictsRevolt.Server.WebApi.Core.Domain.Players;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Resources;

namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Enums;

public class GameSetup
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SessionName { get; set; } = string.Empty;
    public GameMode GameMode { get; set; }
    public GameMode Mode { get; set; } // Legacy property
    public VictoryCondition VictoryCondition { get; set; } = VictoryCondition.Military;
    public List<VictoryCondition> VictoryConditions { get; set; } = new();
    public List<PlayerSetup> Players { get; set; } = new();

    // Galaxy configuration
    public GalaxySize GalaxySize { get; set; } = GalaxySize.Medium;
    public int GalaxySizePlanets { get; set; } = 15; // Number of planets (legacy)
    public int StarSystemCount { get; set; } = 5;
    public int MaxPlayers { get; set; } = 4;

    // Game settings
    public int MaxTurns { get; set; } = 100;
    public bool EnableAI { get; set; } = true;
    public bool EnableDiplomacy { get; set; } = false; // Future feature

    // Resource settings
    public Dictionary<ResourceType, int> StartingResources { get; set; } = new();
    public int StartingCreditsPerPlayer { get; set; } = 1000; // Legacy
    public int StartingMaterialsPerPlayer { get; set; } = 500; // Legacy
    public int StartingFuelPerPlayer { get; set; } = 200; // Legacy

    // AI settings
    public AiDifficulty AiDifficulty { get; set; } = AiDifficulty.Normal;
    public AiDifficulty DefaultAiDifficulty { get; set; } = AiDifficulty.Normal; // Legacy
    public int AiTurnDelaySeconds { get; set; } = 3;
}