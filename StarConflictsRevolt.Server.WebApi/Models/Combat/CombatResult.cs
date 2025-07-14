namespace StarConflictsRevolt.Server.WebApi.Models.Combat;

public class CombatResult
{
    public Guid CombatId { get; set; }
    public CombatType Type { get; set; }
    public bool AttackerVictory { get; set; }
    public int RoundsFought { get; set; }
    public TimeSpan Duration { get; set; }

    // Casualties
    public List<CombatShip> AttackerLosses { get; set; } = new();
    public List<CombatShip> DefenderLosses { get; set; } = new();

    // Rewards and consequences
    public List<CombatReward> Rewards { get; set; } = new();
    public List<CombatConsequence> Consequences { get; set; } = new();

    // Cinematic data for UI
    public CombatCinematicData CinematicData { get; set; } = new();
}