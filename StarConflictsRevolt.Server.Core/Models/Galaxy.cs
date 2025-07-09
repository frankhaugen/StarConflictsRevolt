namespace StarConflictsRevolt.Server.Core.Models;

public record Galaxy(Guid Id, IEnumerable<StarSystem> StarSystems) : GameObject;