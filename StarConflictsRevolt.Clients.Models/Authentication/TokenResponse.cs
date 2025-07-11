namespace StarConflictsRevolt.Clients.Models.Authentication;

public class TokenResponse
{
    public string AccessToken { get; set; }
    public TokenType TokenType { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? Scope { get; set; }
}

public enum TokenType
{
    None = 0,
    Bearer,
    Refresh
}