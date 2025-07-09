namespace StarConflictsRevolt.Server.Core.Models;

public record Planet(Guid Id, string Name, double Radius, double Mass, double RotationSpeed, double OrbitSpeed, double DistanceFromSun) : GameObject
{
    public Planet() : this(Guid.Empty, string.Empty, 0, 0, 0, 0, 0) { }
};