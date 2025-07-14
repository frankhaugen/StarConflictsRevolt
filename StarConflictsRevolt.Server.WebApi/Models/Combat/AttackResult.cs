namespace StarConflictsRevolt.Server.WebApi.Models.Combat;

public class AttackResult
{
    public bool Hit { get; set; }
    public int ShieldDamage { get; set; }
    public int HullDamage { get; set; }
    public bool Critical { get; set; }
    public string Description { get; set; } = string.Empty;
    
    public static AttackResult Miss => new() { Hit = false, Description = "Attack missed" };
    
    public static AttackResult CreateHit(int shieldDamage, int hullDamage, bool critical = false)
    {
        return new AttackResult
        {
            Hit = true,
            ShieldDamage = shieldDamage,
            HullDamage = hullDamage,
            Critical = critical,
            Description = critical ? "Critical hit!" : "Attack hit"
        };
    }
}