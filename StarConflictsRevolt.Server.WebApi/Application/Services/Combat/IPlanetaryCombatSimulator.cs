using StarConflictsRevolt.Server.WebApi.Models;
using StarConflictsRevolt.Server.WebApi.Models.Combat;

namespace StarConflictsRevolt.Server.WebApi.Services.Combat;

public interface IPlanetaryCombatSimulator
{
    CombatResult SimulatePlanetaryCombat(Fleet attacker, Planet defender);
    BombardmentResult ResolveOrbitalBombardment(Fleet attacker, Planet defender);
    GroundCombatResult ResolveGroundCombat(GroundCombat combat, Planet planet);
    CaptureResult DeterminePlanetaryCapture(GroundCombatResult combat, Planet planet);
}