namespace StarConflictsRevolt.Clients.Models;

public interface IGameObject
{
    Guid Id { get; }

    public static Guid CreateId()
    {
        return Guid.CreateVersion7();
    }
}