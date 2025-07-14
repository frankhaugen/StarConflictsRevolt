using StarConflictsRevolt.Server.WebApi.Core.Domain.Enums;

namespace StarConflictsRevolt.Server.WebApi.Datastore.Entities;

public abstract class GameObject : IGameObject
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
}