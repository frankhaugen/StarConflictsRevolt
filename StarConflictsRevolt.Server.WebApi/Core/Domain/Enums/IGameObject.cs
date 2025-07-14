namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Enums;

public interface IGameObject
{
    Guid Id { get; }

    public static Guid CreateId()
    {
        return Guid.CreateVersion7();
    }
}