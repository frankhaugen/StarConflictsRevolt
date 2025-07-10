using StarConflictsRevolt.Server.WebApi.Enums;

namespace StarConflictsRevolt.Server.WebApi.Datastore.Entities;

public class Fleet : GameObject
{
    public Fleet(Guid parse, string rebelFlagship, List<Ship> ships, FleetStatus idle)
    {
        Id = parse;
        Name = rebelFlagship;
        Ships = ships;
        Status = idle;
    }

    public Fleet()
    {
        // Default constructor for serialization
    }

    public string Name { get; set; }
    public Guid OwnerId { get; set; }
    public IEnumerable<Ship> Ships { get; set; }
    public FleetStatus Status { get; set; }
}