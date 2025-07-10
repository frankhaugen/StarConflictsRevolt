namespace StarConflictsRevolt.Server.Core.Models;

public record Galaxy(
    List<StarSystem> StarSystems
) : GameObject;