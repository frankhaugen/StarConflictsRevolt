namespace StarConflictsRevolt.Clients.Models;

public class SessionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}