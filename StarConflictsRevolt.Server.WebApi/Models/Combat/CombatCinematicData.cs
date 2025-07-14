namespace StarConflictsRevolt.Server.WebApi.Models.Combat;

public class CombatCinematicData
{
    public List<string> Highlights { get; set; } = new();
    public List<string> CriticalMoments { get; set; } = new();
    public string FinalNarrative { get; set; } = string.Empty;
    public Dictionary<string, object> CustomData { get; set; } = new();
}