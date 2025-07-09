using StarConflictsRevolt.Server.Core.Enums;

namespace StarConflictsRevolt.Server.Datastore.Entities;

public class Structure : GameObject
{
    public string Name { get; set; }
    public StructureVariant Variant { get; set; }
    public Guid? PlanetId { get; set; }
    public Planet? Planet { get; set; }
}