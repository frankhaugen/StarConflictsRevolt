namespace StarConflictsRevolt.Clients.Models;

public record PlanetDto(
    Guid Id,
    string Name,
    double Radius,
    double Mass,
    double RotationSpeed,
    double OrbitSpeed,
    double DistanceFromSun,
    IReadOnlyList<FleetDto>? Fleets = null
) : IGameObject
{
    public PlanetDto() : this(Guid.Empty, string.Empty, 0, 0, 0, 0, 0, null)
    {
    }

    public IReadOnlyList<FleetDto> FleetsOrEmpty => Fleets ?? Array.Empty<FleetDto>();
}