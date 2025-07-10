namespace StarConflictsRevolt.Clients.Shared;

public class TokenProviderOptions
{
    public string TokenEndpoint { get; set; } = "http://localhost:5153/token";
    public string ClientId { get; set; }
    public string Secret { get; set; } = "changeme";
}