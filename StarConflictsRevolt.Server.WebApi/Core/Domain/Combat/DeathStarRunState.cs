namespace StarConflictsRevolt.Server.WebApi.Models.Combat;

public class DeathStarRunState
{
    public RunPhase Phase { get; set; } = RunPhase.Approach;
    public int TrenchPosition { get; set; } = 0;
    public List<TurbolaserShot> TurbolaserFire { get; set; } = new();
    public List<TIEFighter> TIEInterceptors { get; set; } = new();
    public List<CombatShip> HeroPilots { get; set; } = new();
    public bool ShieldGeneratorDestroyed { get; set; } = false;
    public bool ExhaustPortVulnerable { get; set; } = false;
    public int SurvivingShips { get; set; } = 0;
}