using StarConflictsRevolt.Server.WebApi.Models;

namespace StarConflictsRevolt.Server.WebApi.Datastore.Entities;

public abstract class GameObject : IGameObject
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
}