using StarConflictsRevolt.Server.WebApi.Enums;

namespace StarConflictsRevolt.Server.WebApi.Datastore.Entities;

public class Structure : GameObject
{
    public StructureVariant Variant { get; set; }
    public Guid? PlanetId { get; set; }
    public Planet? Planet { get; set; }
}