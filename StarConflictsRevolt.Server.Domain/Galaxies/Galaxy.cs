using StarConflictsRevolt.Server.Domain.Gameplay;
using StarSystem = StarConflictsRevolt.Server.Domain.Stars.StarSystem;

namespace StarConflictsRevolt.Server.Domain.Galaxies;

public record Galaxy(
    List<StarSystem> StarSystems
) : GameObject;