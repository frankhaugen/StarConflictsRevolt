namespace StarConflictsRevolt.Server.WebApi.Models;

public record Planet(
    string Name,
    double Radius,
    double Mass,
    double RotationSpeed,
    double OrbitSpeed,
    double DistanceFromSun,
    List<Fleet> Fleets,
    List<Structure> Structures
) : GameObject;