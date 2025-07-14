using System.Numerics;

namespace StarConflictsRevolt.Server.WebApi.Models;

public record StarSystem(
    Guid Id,
    string Name,
    List<Planet> Planets,
    Vector2 Coordinates
);