namespace StarConflictsRevolt.Clients.Models;

public class GameStateInfoDto
{
    public Guid PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int Credits { get; set; }
    public int Materials { get; set; }
    public int Fuel { get; set; }
    // Add other session-relevant fields as needed
} 