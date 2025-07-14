namespace StarConflictsRevolt.Clients.Models;

public class LeaderboardDto
{
    public Guid SessionId { get; set; }
    public List<PlayerStatsDto> Players { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

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

public class TopPlayersDto
{
    public List<PlayerStatsDto> TopPlayers { get; set; } = new();
    public int TotalPlayers { get; set; }
    public DateTime LastUpdated { get; set; }
} 