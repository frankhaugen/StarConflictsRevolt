using StarConflictsRevolt.Server.Core.Models;

namespace StarConflictsRevolt.Server.Datastore.Entities;

public abstract class GameObject : IGameObject
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
}