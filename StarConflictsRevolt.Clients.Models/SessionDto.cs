namespace StarConflictsRevolt.Clients.Models;

public class SessionDto
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string SessionName { get; set; }
    public bool IsActive { get; set; }
}