using System.Numerics;
using System.Collections.Generic;

namespace StarConflictsRevolt.Server.Core.Models;

public record StarSystem(
    Guid Id,
    string Name,
    List<Planet> Planets,
    Vector2 Coordinates
);