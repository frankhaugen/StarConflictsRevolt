using System.Numerics;

namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Gameplay;

public class StarSystem : Datastore.Entities.GameObject
{
    public string Name { get; set; }
    public IEnumerable<Planet> Planets { get; set; }
    public Vector2 Coordinates { get; set; }
}