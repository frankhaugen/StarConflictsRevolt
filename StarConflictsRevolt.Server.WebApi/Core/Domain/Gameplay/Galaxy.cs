namespace StarConflictsRevolt.Server.WebApi.Datastore.Entities;

public class Galaxy : GameObject
{
    public IEnumerable<StarSystem> StarSystems { get; set; }
}