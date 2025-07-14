namespace StarConflictsRevolt.Clients.Models;

public class SessionInfo
{
    public Guid Id { get; set; }
    public string SessionName { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public string SessionType { get; set; } = string.Empty;
    public int PlayerCount { get; set; }
} 