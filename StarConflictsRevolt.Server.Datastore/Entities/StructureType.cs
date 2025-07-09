using StarConflictsRevolt.Server.Core;
using StarConflictsRevolt.Server.Core.Enums;

namespace StarConflictsRevolt.Server.Datastore.Entities;

public class StructureType : GameObject
{
    /// <inheritdoc />
    public StructureVariant Variant { get; set; }
}