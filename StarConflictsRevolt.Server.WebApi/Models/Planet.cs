namespace StarConflictsRevolt.Server.WebApi.Models;

public record Planet(
    string Name,
    double Radius,
    double Mass,
    double RotationSpeed,
    double OrbitSpeed,
    double DistanceFromSun,
    List<Fleet> Fleets,
    List<Structure> Structures,
    Guid? OwnerId = null,
    int Population = 1000,
    int MaxPopulation = 10000,
    int Minerals = 100,
    int MaxMinerals = 1000,
    int Energy = 50,
    int MaxEnergy = 500,
    double ProductionRate = 1.0
) : GameObject;