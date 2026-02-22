namespace StarConflictsRevolt.Server.WebApi.Infrastructure.Security;

public class CreateSessionRequest
{
    public string SessionName { get; set; } = string.Empty;
    public string? SessionType { get; set; }
    /// <summary>Optional client/player id for player tracking. When set with SinglePlayer, server may return existing session instead of creating a new world.</summary>
    public string? ClientId { get; set; }
}