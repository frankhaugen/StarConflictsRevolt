using StarConflictsRevolt.Server.Domain.Enums;

namespace StarConflictsRevolt.Server.Domain.Gameplay;

public class Structure : GameObjectBase
{
    public StructureVariant Variant { get; set; }
    public Guid? PlanetId { get; set; }
    public Planet? Planet { get; set; }
}