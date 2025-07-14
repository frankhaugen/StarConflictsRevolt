namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Players;

public class PlayerState
{
    public Guid PlayerId { get; set; }
    public int Credits { get; set; }
    public List<string> ResearchedTechnologies { get; set; } = new();
}