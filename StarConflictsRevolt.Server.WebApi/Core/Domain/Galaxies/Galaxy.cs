namespace StarConflictsRevolt.Server.WebApi.Models;

public record Galaxy(
    List<StarSystem> StarSystems
) : GameObject;