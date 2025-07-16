namespace StarConflictsRevolt.Clients.Shared.Player;

public record PlayerProfile
{
    public string Name { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime LastAccessed { get; init; }
} 