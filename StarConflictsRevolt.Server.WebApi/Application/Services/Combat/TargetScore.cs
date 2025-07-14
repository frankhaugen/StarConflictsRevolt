using StarConflictsRevolt.Server.WebApi.Core.Domain.Combat;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Combat;

public class TargetScore
{
    public CombatShip Ship { get; set; } = new();
    public double Score { get; set; }
}