using StarConflictsRevolt.Server.WebApi.Core.Domain.Gameplay;
using StarSystem = StarConflictsRevolt.Server.WebApi.Core.Domain.Stars.StarSystem;

namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Galaxies;

public record Galaxy(
    List<StarSystem> StarSystems
) : GameObject;