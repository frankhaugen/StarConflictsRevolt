using StarConflictsRevolt.Server.WebApi.Models.Combat;

namespace StarConflictsRevolt.Server.WebApi.Services.Combat;

public class TargetScore
{
    public CombatShip Ship { get; set; } = new();
    public double Score { get; set; }
}