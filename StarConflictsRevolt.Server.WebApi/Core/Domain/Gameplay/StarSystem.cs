using System.Numerics;

namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Gameplay;

public class StarSystem : Infrastructure.Datastore.Entities.GameObject
{
    public string Name { get; set; } = string.Empty;
    public IEnumerable<Planet> Planets { get; set; } = Array.Empty<Planet>();
    public Vector2 Coordinates { get; set; }
}