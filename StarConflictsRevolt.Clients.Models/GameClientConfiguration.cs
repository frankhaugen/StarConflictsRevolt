namespace StarConflictsRevolt.Clients.Models;

public class GameClientConfiguration
{
    public string GameServerUrl { get; set; } = "http://localhost:5153";
    public string GameServerHubUrl { get; set; } = "http://localhost:5267/gamehub";
}