namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Gameplay;

public class Planet : Datastore.Entities.GameObject
{
    public string Name { get; set; }
    public double Radius { get; set; }
    public double Mass { get; set; }
    public double RotationSpeed { get; set; }
    public double OrbitSpeed { get; set; }
    public double DistanceFromSun { get; set; }
}