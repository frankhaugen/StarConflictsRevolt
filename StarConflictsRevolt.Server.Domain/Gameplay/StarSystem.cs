using System.Numerics;

namespace StarConflictsRevolt.Server.Domain.Gameplay;

public class StarSystem : GameObjectBase
{
    public string Name { get; set; } = string.Empty;
    public IEnumerable<Planet> Planets { get; set; } = Array.Empty<Planet>();
    public Vector2 Coordinates { get; set; }
}