namespace StarConflictsRevolt.Server.WebApi.Models.Combat;

public class GroundCombatResult
{
    public bool AttackerVictory { get; set; }
    public int RoundsFought { get; set; }
    public TimeSpan Duration { get; set; }
    
    // Casualties
    public List<GroundUnit> AttackerLosses { get; set; } = new();
    public List<GroundUnit> DefenderLosses { get; set; } = new();
    
    // Planetary effects
    public int PopulationCasualties { get; set; }
    public int InfrastructureDamage { get; set; }
    public List<Structure> DestroyedStructures { get; set; } = new();
    
    // Capture result
    public CaptureResult CaptureResult { get; set; } = new();
    
    // Cinematic data
    public GroundCombatCinematicData CinematicData { get; set; } = new();
}