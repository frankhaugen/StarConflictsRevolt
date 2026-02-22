using StarConflictsRevolt.Server.Domain.Enums;

namespace StarConflictsRevolt.Server.Domain.Players;

public class PlayerSetup
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public PlayerType Type { get; set; }
    public AiDifficulty Difficulty { get; set; } = AiDifficulty.Normal;
    public string Color { get; set; } = "#FFFFFF";
    public Guid? StartingPlanetId { get; set; }
    /// <summary>Application layer sets this (e.g. IAiStrategy); Simulation does not reference it.</summary>
    public object? AiStrategy { get; set; }

    // Starting resources
    public int StartingCredits { get; set; } = 1000;
    public int StartingMaterials { get; set; } = 500;
    public int StartingFuel { get; set; } = 200;
}