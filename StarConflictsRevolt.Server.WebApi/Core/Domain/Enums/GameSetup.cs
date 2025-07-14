using StarConflictsRevolt.Server.WebApi.Core.Domain.Players;

namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Enums;

public class GameSetup
{
    public string SessionName { get; set; } = string.Empty;
    public GameMode Mode { get; set; }
    public Victory.VictoryCondition VictoryCondition { get; set; } = Victory.VictoryCondition.MilitaryVictory;
    public List<PlayerSetup> Players { get; set; } = new();

    // Galaxy configuration
    public int GalaxySize { get; set; } = 15; // Number of planets
    public int StarSystemCount { get; set; } = 5;

    // Game settings
    public int MaxTurns { get; set; } = 100;
    public bool EnableAI { get; set; } = true;
    public bool EnableDiplomacy { get; set; } = false; // Future feature

    // Resource settings
    public int StartingCreditsPerPlayer { get; set; } = 1000;
    public int StartingMaterialsPerPlayer { get; set; } = 500;
    public int StartingFuelPerPlayer { get; set; } = 200;

    // AI settings
    public AiDifficulty DefaultAiDifficulty { get; set; } = AiDifficulty.Normal;
    public int AiTurnDelaySeconds { get; set; } = 3;
}