using System.Numerics;

namespace StarConflictsRevolt.Server.Core;

public record StarSystem(Guid Id, string Name, IEnumerable<Planet> Planets, Vector2 Coordinates) : GameObject;