using System.Text.Json.Serialization;

namespace StarConflictsRevolt.Clients.Models.Authentication;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TokenType
{
    None = 0,
    Bearer,
    Refresh
}