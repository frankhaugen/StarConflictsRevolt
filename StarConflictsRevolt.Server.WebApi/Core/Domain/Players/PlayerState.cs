namespace StarConflictsRevolt.Server.WebApi.Models;

public class PlayerState
{
    public Guid PlayerId { get; set; }
    public int Credits { get; set; }
    public List<string> ResearchedTechnologies { get; set; } = new();
}