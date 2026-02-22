using StarConflictsRevolt.Server.Domain.Enums;

namespace StarConflictsRevolt.Server.Domain.Gameplay;

public class Fleet : GameObjectBase
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

    public string Name { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public IEnumerable<Ship> Ships { get; set; } = Array.Empty<Ship>();
    public FleetStatus Status { get; set; }
}