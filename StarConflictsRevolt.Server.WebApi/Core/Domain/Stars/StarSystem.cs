using System.Numerics;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Planets;

namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Stars;

public record StarSystem(
    Guid Id,
    string Name,
    List<Planet> Planets,
    Vector2 Coordinates
);