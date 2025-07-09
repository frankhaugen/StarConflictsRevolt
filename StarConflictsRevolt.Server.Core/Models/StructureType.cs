using StarConflictsRevolt.Server.Core.Enums;

namespace StarConflictsRevolt.Server.Core.Models;

public record StructureType(
    Guid Id,
    StructureVariant Variant
);