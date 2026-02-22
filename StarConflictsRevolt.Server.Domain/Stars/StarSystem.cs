using System.Numerics;
using StarConflictsRevolt.Server.Domain.Planets;

namespace StarConflictsRevolt.Server.Domain.Stars;

public record StarSystem(
    Guid Id,
    string Name,
    List<Planet> Planets,
    Vector2 Coordinates
);