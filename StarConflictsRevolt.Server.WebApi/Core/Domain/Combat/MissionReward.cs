namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Combat;

public class MissionReward
{
    public RewardType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Value { get; set; }
    public double Probability { get; set; } = 1.0; // Chance of receiving this reward
    public Guid? TargetId { get; set; }
}