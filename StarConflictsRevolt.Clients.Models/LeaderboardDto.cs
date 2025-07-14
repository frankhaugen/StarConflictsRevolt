namespace StarConflictsRevolt.Clients.Models;

public class LeaderboardDto
{
    public Guid SessionId { get; set; }
    public List<PlayerStatsDto> Players { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}