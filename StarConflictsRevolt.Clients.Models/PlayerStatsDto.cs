namespace StarConflictsRevolt.Clients.Models;

public class PlayerStatsDto
{
    public Guid PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int BattlesWon { get; set; }
    public int BattlesLost { get; set; }
    public int StructuresBuilt { get; set; }
    public int PlanetsControlled { get; set; }
    public int TotalScore { get; set; }
    public DateTime LastActive { get; set; }
}