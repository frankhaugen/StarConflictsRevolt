namespace StarConflictsRevolt.Clients.Models;

public class GameClientConfiguration
{
    public string GameServerUrl { get; set; } = "https://localhost:5001";
    public string GameServerHubUrl { get; set; } = "https://localhost:5001/gamehub";
}