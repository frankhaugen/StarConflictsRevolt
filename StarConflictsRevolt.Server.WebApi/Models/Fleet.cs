using StarConflictsRevolt.Server.WebApi.Enums;

namespace StarConflictsRevolt.Server.WebApi.Models;

public record Fleet(
    Guid Id,
    string Name,
    List<Ship> Ships,
    Guid? LocationPlanetId,
    Guid OwnerId,
    FleetStatus Status = FleetStatus.Idle,
    Guid? DestinationPlanetId = null,
    DateTime? DepartureTime = null,
    DateTime? ArrivalTime = null
) : GameObject;