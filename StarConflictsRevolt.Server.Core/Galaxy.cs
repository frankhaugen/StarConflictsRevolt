namespace StarConflictsRevolt.Server.Core;

public record Galaxy(Guid Id, IEnumerable<StarSystem> StarSystems) : GameObject;