using StarConflictsRevolt.Server.Domain.Combat;

namespace StarConflictsRevolt.Server.Combat;

public class TargetScore
{
    public CombatShip Ship { get; set; } = new();
    public double Score { get; set; }
}