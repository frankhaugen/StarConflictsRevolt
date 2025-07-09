using StarConflictsRevolt.Server.Core.Enums;

namespace StarConflictsRevolt.Server.Core.Models;

public record Structure(string Name, StructureVariant Variant, Planet Pla) : GameObject;