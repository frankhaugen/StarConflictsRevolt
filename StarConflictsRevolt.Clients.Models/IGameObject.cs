namespace StarConflictsRevolt.Clients.Shared;

public interface IGameObject
{
    Guid Id { get; }
    
    public static Guid CreateId() => Guid.CreateVersion7();
}