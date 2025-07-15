namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Gameplay;

public class Galaxy : Infrastructure.Datastore.Entities.GameObject
{
    public IEnumerable<StarSystem> StarSystems { get; set; }
}