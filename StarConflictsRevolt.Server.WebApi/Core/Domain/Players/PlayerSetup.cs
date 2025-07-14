using StarConflictsRevolt.Server.WebApi.Enums;
using StarConflictsRevolt.Server.WebApi.Services;

namespace StarConflictsRevolt.Server.WebApi.Models;

public class PlayerSetup
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public PlayerType Type { get; set; }
    public AiDifficulty Difficulty { get; set; } = AiDifficulty.Normal;
    public string Color { get; set; } = "#FFFFFF";
    public Guid? StartingPlanetId { get; set; }
    public IAiStrategy? AiStrategy { get; set; }

    // Starting resources
    public int StartingCredits { get; set; } = 1000;
    public int StartingMaterials { get; set; } = 500;
    public int StartingFuel { get; set; } = 200;
}