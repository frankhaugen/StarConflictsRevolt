namespace StarConflictsRevolt.Clients.Models;

public class GameClientConfiguration
{
    public string GameServerUrl { get; set; } = "http://localhost:5153";
    public string GameServerHubUrl { get; set; } = "http://localhost:5153/gamehub";
    /// <summary>Optional. When set (e.g. by Aspire), client can use GameHub for commands.</summary>
    public string? CommandHubUrl { get; set; }
    public string ApiBaseUrl { get; set; } = "http://localhost:5153";
}