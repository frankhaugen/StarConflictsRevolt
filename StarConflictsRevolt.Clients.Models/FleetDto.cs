namespace StarConflictsRevolt.Clients.Models;

/// <summary>
/// Client-side representation of a fleet (ships at a location or in transit).
/// </summary>
public record FleetDto(
    Guid Id,
    string Name,
    int ShipCount,
    Guid? LocationPlanetId,
    Guid OwnerId,
    string Status, // Idle, Moving, EnRoute, etc.
    Guid? DestinationPlanetId,
    DateTime? ArrivalTime
) : IGameObject;
