using StarConflictsRevolt.Server.Core.Enums;

namespace StarConflictsRevolt.Server.Core.Models;

public record Fleet(
    Guid Id,
    string Name,
    List<Ship> Ships,
    Guid? LocationPlanetId
) : GameObject;