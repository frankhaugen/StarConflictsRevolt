namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Missions;

public class MissionConsequence
{
    public ConsequenceType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Severity { get; set; } = 1; // 1-10 scale
    public int Amount { get; set; } = 0;
    public string? TargetId { get; set; }
    public Dictionary<string, object> AdditionalData { get; set; } = new();
    
    public MissionConsequence()
    {
        // Default constructor for serialization
    }
    
    public MissionConsequence(ConsequenceType type, string description, int severity = 1, int amount = 0)
    {
        Type = type;
        Description = description;
        Severity = severity;
        Amount = amount;
    }
}