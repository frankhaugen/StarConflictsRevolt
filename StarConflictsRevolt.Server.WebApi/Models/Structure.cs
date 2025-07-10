using StarConflictsRevolt.Server.Core.Enums;

namespace StarConflictsRevolt.Server.Core.Models;

public record Structure(StructureVariant Variant, Planet Planet) : GameObject;