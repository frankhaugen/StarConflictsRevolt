using StarConflictsRevolt.Server.WebApi.Enums;

namespace StarConflictsRevolt.Server.WebApi.Models;

public record Structure(StructureVariant Variant, Planet Planet) : GameObject;