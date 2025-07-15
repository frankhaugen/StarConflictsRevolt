namespace StarConflictsRevolt.Clients.Shared.Authentication.Configuration;

public class TokenProviderOptions
{
    public string TokenEndpoint { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public TimeSpan TokenExpiry { get; set; } = TimeSpan.FromHours(1);
}