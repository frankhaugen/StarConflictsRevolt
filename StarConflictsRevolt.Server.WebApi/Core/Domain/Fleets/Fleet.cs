using StarConflictsRevolt.Server.WebApi.Core.Domain.Enums;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Gameplay;

namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Fleets;

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