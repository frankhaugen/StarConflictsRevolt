namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Missions;

public class MissionReward
{
    public RewardType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Amount { get; set; } = 0;
    public string? ItemId { get; set; }
    public Dictionary<string, object> AdditionalData { get; set; } = new();
    
    public MissionReward()
    {
        // Default constructor for serialization
    }
    
    public MissionReward(RewardType type, string description, int amount = 0)
    {
        Type = type;
        Description = description;
        Amount = amount;
    }
}