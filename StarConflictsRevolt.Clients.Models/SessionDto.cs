namespace StarConflictsRevolt.Clients.Models;

public class SessionDto
{
    public Guid Id { get; set; }
    public string SessionName { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public bool IsActive { get; set; }
    public DateTime? Ended { get; set; }
    public string SessionType { get; set; } = "Multiplayer"; // Default to multiplayer
}