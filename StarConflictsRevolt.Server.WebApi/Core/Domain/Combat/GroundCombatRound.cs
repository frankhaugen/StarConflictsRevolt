namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Combat;

public class GroundCombatRound
{
    public int RoundNumber { get; set; }
    public List<GroundCombatAction> Actions { get; set; } = new();
    public List<GroundUnit> DestroyedUnits { get; set; } = new();
    public bool CombatEnded { get; set; } = false;
    public string? EndReason { get; set; }
}