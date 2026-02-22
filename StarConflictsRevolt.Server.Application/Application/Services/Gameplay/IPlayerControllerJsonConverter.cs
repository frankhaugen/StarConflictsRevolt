using System.Text.Json;
using System.Text.Json.Serialization;
using StarConflictsRevolt.Server.Domain;

namespace StarConflictsRevolt.Server.Application.Services.Gameplay;

/// <summary>
/// Allows System.Text.Json to deserialize <see cref="IPlayerController"/> (e.g. in World.Players)
/// by round-tripping as the concrete type used for snapshot/clone (identity only; no AI or commands).
/// </summary>
public sealed class IPlayerControllerJsonConverter : JsonConverter<IPlayerController>
{
    public override IPlayerController? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return null;
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject for IPlayerController.");
        Guid playerId = default;
        string? name = null;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject) break;
            if (reader.TokenType != JsonTokenType.PropertyName) continue;
            var prop = reader.GetString();
            reader.Read();
            if (string.Equals(prop, "playerId", StringComparison.OrdinalIgnoreCase))
                playerId = JsonSerializer.Deserialize<Guid>(ref reader, options);
            else if (string.Equals(prop, "name", StringComparison.OrdinalIgnoreCase))
                name = reader.GetString();
        }
        return new PlayerController { PlayerId = playerId, Name = name ?? "Unknown" };
    }

    public override void Write(Utf8JsonWriter writer, IPlayerController value, JsonSerializerOptions options)
    {
        if (value == null) { writer.WriteNullValue(); return; }
        writer.WriteStartObject();
        writer.WriteString("playerId", value.PlayerId);
        writer.WriteString("name", value.Name);
        writer.WriteEndObject();
    }
}
