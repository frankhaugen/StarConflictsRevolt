namespace StarConflictsRevolt.Server.WebApi.Models.Combat;

public class CombatRound
{
    public int RoundNumber { get; set; }
    public List<CombatAction> Actions { get; set; } = new();
    public List<CombatShip> DestroyedShips { get; set; } = new();
    public bool CombatEnded { get; set; } = false;
    public string? EndReason { get; set; }
}