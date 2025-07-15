using StarConflictsRevolt.Server.WebApi.Core.Domain.Enums;

namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Gameplay;

public class Structure : Infrastructure.Datastore.Entities.GameObject
{
    public StructureVariant Variant { get; set; }
    public Guid? PlanetId { get; set; }
    public Planet? Planet { get; set; }
}