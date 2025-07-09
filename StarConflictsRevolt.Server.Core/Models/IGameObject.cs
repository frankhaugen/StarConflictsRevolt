namespace StarConflictsRevolt.Server.Core.Models;

public interface IGameObject
{
    Guid Id { get; }

    public static Guid CreateId() => Guid.CreateVersion7();
}