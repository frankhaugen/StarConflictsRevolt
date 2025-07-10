namespace StarConflictsRevolt.Server.WebApi.Models;

public record Fleet(
    Guid Id,
    string Name,
    List<Ship> Ships,
    Guid? LocationPlanetId
) : GameObject;