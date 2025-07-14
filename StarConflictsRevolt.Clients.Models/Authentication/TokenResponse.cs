using System.Text.Json.Serialization;

namespace StarConflictsRevolt.Clients.Models.Authentication;

public class TokenResponse
{
    [JsonPropertyName("access_token")] public string AccessToken { get; set; }

    [JsonPropertyName("token_type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TokenType TokenType { get; set; }

    [JsonPropertyName("expires_at")] public DateTime ExpiresAt { get; set; }

    public string? Scope { get; set; }
}