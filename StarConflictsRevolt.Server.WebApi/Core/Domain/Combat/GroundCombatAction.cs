namespace StarConflictsRevolt.Server.WebApi.Models.Combat;

public class GroundCombatAction
{
    public Guid ActorId { get; set; }
    public Guid? TargetId { get; set; }
    public GroundActionType Type { get; set; }
    public GroundAttackResult? AttackResult { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}