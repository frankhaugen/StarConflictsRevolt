namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Combat;

public class Mission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public MissionType Type { get; set; }
    public int BaseDifficulty { get; set; }
    public int Duration { get; set; } // in turns
    public List<MissionRequirement> Requirements { get; set; } = new();
    public List<MissionReward> PotentialRewards { get; set; } = new();
    public List<MissionConsequence> PotentialConsequences { get; set; } = new();
    public bool IsAvailable { get; set; } = true;
    public Guid? AssignedAgentId { get; set; }
    public Guid? TargetPlanetId { get; set; }
    public MissionStatus Status { get; set; } = MissionStatus.Available;
    public DateTime? StartTime { get; set; }
    public DateTime? CompletionTime { get; set; }
}