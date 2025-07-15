namespace StarConflictsRevolt.Clients.Shared;

public record UserProfile
{
    public string UserId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
}