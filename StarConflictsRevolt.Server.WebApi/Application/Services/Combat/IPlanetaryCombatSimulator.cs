using StarConflictsRevolt.Server.WebApi.Core.Domain.Combat;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Fleets;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Planets;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Combat;

public interface IPlanetaryCombatSimulator
{
    CombatResult SimulatePlanetaryCombat(Fleet attacker, Planet defender);
    BombardmentResult ResolveOrbitalBombardment(Fleet attacker, Planet defender);
    GroundCombatResult ResolveGroundCombat(GroundCombat combat, Planet planet);
    CaptureResult DeterminePlanetaryCapture(GroundCombatResult combat, Planet planet);
}