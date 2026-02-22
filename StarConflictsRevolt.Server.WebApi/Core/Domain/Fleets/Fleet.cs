using StarConflictsRevolt.Server.WebApi.Core.Domain.Enums;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Gameplay;

namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Fleets;

#pragma warning disable CS8907, CS8618 // Record primary constructor; callers supply all values
public record Fleet(
    Guid Id,
    string Name,
    List<Ship> Ships,
    Guid? LocationPlanetId,
    Guid OwnerId,
    FleetStatus Status = FleetStatus.Idle,
    Guid? DestinationPlanetId = null,
    DateTime? DepartureTime = null,
    DateTime? ArrivalTime = null,
    long? EtaTick = null
) : GameObject;
#pragma warning restore CS8907