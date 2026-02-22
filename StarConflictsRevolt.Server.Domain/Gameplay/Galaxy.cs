namespace StarConflictsRevolt.Server.Domain.Gameplay;

public class Galaxy : GameObjectBase
{
    public IEnumerable<StarSystem> StarSystems { get; set; } = Array.Empty<StarSystem>();
}