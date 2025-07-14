namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Gameplay;

public class Galaxy : Datastore.Entities.GameObject
{
    public IEnumerable<StarSystem> StarSystems { get; set; }
}