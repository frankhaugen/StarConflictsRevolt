namespace StarConflictsRevolt.Server.WebApi.Security;

public class TokenRequest
{
    public string ClientId { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
}

public class CreateSessionRequest
{
    public string SessionName { get; set; } = string.Empty;
    public string? SessionType { get; set; }
}