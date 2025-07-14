namespace StarConflictsRevolt.Server.WebApi.Models.Combat;

public class CombatAction
{
    public Guid ActorId { get; set; }
    public Guid? TargetId { get; set; }
    public ActionType Type { get; set; }
    public AttackResult? AttackResult { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}