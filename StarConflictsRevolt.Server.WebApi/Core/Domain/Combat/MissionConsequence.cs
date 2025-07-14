namespace StarConflictsRevolt.Server.WebApi.Models.Combat;

public class MissionConsequence
{
    public ConsequenceType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Value { get; set; }
    public double Probability { get; set; } = 1.0; // Chance of this consequence occurring
    public Guid? TargetId { get; set; }
}