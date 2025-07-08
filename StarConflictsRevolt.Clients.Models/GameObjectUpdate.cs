using System.Text.Json;

namespace StarConflictsRevolt.Clients.Models;

public record GameObjectUpdate(Guid Id, UpdateType Type, JsonElement? Data)
{
    public GameObjectUpdate(Guid id, UpdateType type, object? data)
        : this(id, type, data is null ? null : JsonSerializer.SerializeToElement(data))
    {
    }
    public GameObjectUpdate(Guid id, UpdateType type)
        : this(id, type, null)
    {
    }
    public GameObjectUpdate() : this(Guid.Empty, UpdateType.Added, null)
    {
    }
}