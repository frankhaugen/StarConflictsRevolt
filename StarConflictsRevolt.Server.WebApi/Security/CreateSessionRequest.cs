namespace StarConflictsRevolt.Server.WebApi.Security;

public class CreateSessionRequest
{
    public string SessionName { get; set; } = string.Empty;
    public string? SessionType { get; set; }
}