namespace StarConflictsRevolt.Clients.Shared;

public record PlanetDto(Guid Id, string Name, double Radius, double Mass, double RotationSpeed, double OrbitSpeed, double DistanceFromSun) : IGameObject
{
    public PlanetDto() : this(Guid.Empty, string.Empty, 0, 0, 0, 0, 0) { }
};