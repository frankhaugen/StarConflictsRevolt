namespace StarConflictsRevolt.Server.WebApi.Models.Combat;

public class GroundAttackResult
{
    public bool Hit { get; set; }
    public int Damage { get; set; }
    public bool Critical { get; set; }
    public bool Suppressed { get; set; }
    public string Description { get; set; } = string.Empty;
    
    public static GroundAttackResult Miss => new() { Hit = false, Description = "Attack missed" };
    
    public static GroundAttackResult CreateHit(int damage, bool critical = false, bool suppressed = false)
    {
        return new GroundAttackResult
        {
            Hit = true,
            Damage = damage,
            Critical = critical,
            Suppressed = suppressed,
            Description = critical ? "Critical hit!" : "Attack hit"
        };
    }
}