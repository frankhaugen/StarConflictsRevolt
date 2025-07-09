using System.Numerics;
using StarConflictsRevolt.Server.Core;

namespace StarConflictsRevolt.Server.Datastore.Entities;

public class StarSystem : GameObject
{
    public string Name { get; set; }
    public IEnumerable<Planet> Planets { get; set; }
    public Vector2 Coordinates { get; set; }
}