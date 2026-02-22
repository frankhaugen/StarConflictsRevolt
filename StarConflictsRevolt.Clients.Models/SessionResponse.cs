namespace StarConflictsRevolt.Clients.Models;

public class SessionResponse
{
    public Guid SessionId { get; set; }
    public WorldDto? World { get; set; }
    /// <summary>Player id for the creating or joining player (use for build-structure, move-fleet, etc.).</summary>
    public Guid? PlayerId { get; set; }
}