namespace StarConflictsRevolt.Server.WebApi.Models.Combat;

public class MissionRequirement
{
    public RequirementType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Value { get; set; }
    public bool IsMet { get; set; } = false;
}