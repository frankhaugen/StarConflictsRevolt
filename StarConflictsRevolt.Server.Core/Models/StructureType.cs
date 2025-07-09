using StarConflictsRevolt.Server.Core.Enums;

namespace StarConflictsRevolt.Server.Core.Models;

public record StructureType : GameObject
{
    /// <inheritdoc />
    public StructureVariant Variant { get; set; }
}