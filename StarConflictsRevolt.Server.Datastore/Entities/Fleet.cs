using StarConflictsRevolt.Server.Core.Enums;

namespace StarConflictsRevolt.Server.Datastore.Entities;

public class Fleet : GameObject
{
    public Fleet(Guid parse, string rebelFlagship, List<Ship> ships, FleetStatus idle, object o)
    {
        Id = parse;
        Name = rebelFlagship;
        Ships = ships;
        Status = idle;
        OrbitingPlanetId = o as Guid?;
    }

    public Fleet()
    {
        // Default constructor for serialization
    }

    public string Name { get; set; }
    public Guid OwnerId { get; set; }
    public IEnumerable<Ship> Ships { get; set; }
    public FleetStatus Status { get; set; }
    public Guid? OrbitingPlanetId { get; set; }
}