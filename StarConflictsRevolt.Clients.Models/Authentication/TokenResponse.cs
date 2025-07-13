namespace StarConflictsRevolt.Clients.Models.Authentication;

using System.Text.Json.Serialization;

public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    [JsonPropertyName("token_type")]
    [JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public TokenType TokenType { get; set; }

    [JsonPropertyName("expires_at")]
    public DateTime ExpiresAt { get; set; }
    public string? Scope { get; set; }
}