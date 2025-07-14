namespace StarConflictsRevolt.Clients.Models;

public class SessionResponse
{
    public Guid SessionId { get; set; }
    public WorldDto? World { get; set; }
}