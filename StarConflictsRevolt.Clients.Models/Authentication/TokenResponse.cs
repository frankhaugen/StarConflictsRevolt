namespace StarConflictsRevolt.Clients.Models.Authentication;

public class TokenResponse
{
    public string AccessToken { get; set; } = string.Empty;

    public TokenType TokenType { get; set; }

    public DateTime ExpiresAt { get; set; }

    public string? Scope { get; set; }
}