using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace StarConflictsRevolt.Clients.Models;

public record GameObjectUpdate(Guid Id, UpdateType Type, JsonElement? Data)
{
    public static GameObjectUpdate Create(Guid id, object? data = null) =>
        new(id, UpdateType.Added, data != null ? JsonSerializer.SerializeToElement(data, Settings) : null);

    public static JsonSerializerOptions Settings = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = JsonTypeInfoResolver.Combine(
            new DefaultJsonTypeInfoResolver()
        ),
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true)
        },
        NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString,
        PropertyNameCaseInsensitive = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    public static GameObjectUpdate Update(Guid id, object? data = null) =>
        new(id, UpdateType.Changed, data != null ? JsonSerializer.SerializeToElement(data, Settings) : null);

    public static GameObjectUpdate Delete(Guid id) =>
        new(id, UpdateType.Removed, null);
}