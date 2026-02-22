namespace StarConflictsRevolt.Server.Domain.Enums;

public interface IGameObject
{
    Guid Id { get; }

    public static Guid CreateId()
    {
        return Guid.CreateVersion7();
    }
}