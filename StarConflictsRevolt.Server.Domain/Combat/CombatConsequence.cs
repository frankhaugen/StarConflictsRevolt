namespace StarConflictsRevolt.Server.Domain.Combat;

public class CombatConsequence
{
    public ConsequenceType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Value { get; set; }
    public Guid? TargetId { get; set; }
}