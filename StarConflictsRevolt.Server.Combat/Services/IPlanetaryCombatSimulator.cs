using StarConflictsRevolt.Server.Domain.Combat;
using StarConflictsRevolt.Server.Domain.Fleets;
using StarConflictsRevolt.Server.Domain.Planets;

namespace StarConflictsRevolt.Server.Combat;

public interface IPlanetaryCombatSimulator
{
    CombatResult SimulatePlanetaryCombat(Fleet attacker, Planet defender);
    BombardmentResult ResolveOrbitalBombardment(Fleet attacker, Planet defender);
    GroundCombatResult ResolveGroundCombat(GroundCombat combat, Planet planet);
    CaptureResult DeterminePlanetaryCapture(GroundCombatResult combat, Planet planet);
}