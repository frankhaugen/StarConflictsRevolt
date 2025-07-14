namespace StarConflictsRevolt.Clients.Models;

public class TopPlayersDto
{
    public List<PlayerStatsDto> TopPlayers { get; set; } = new();
    public int TotalPlayers { get; set; }
    public DateTime LastUpdated { get; set; }
}