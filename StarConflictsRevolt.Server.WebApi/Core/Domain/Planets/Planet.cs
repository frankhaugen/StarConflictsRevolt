using StarConflictsRevolt.Server.WebApi.Core.Domain.Gameplay;
using Fleet = StarConflictsRevolt.Server.WebApi.Core.Domain.Fleets.Fleet;
using Structure = StarConflictsRevolt.Server.WebApi.Core.Domain.Structures.Structure;

namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Planets;

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
    double ProductionRate = 1.0,
    PlanetType PlanetType = null,
    int Credits = 0,
    int Materials = 0,
    int Fuel = 0
) : GameObject;