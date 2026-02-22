using StarConflictsRevolt.Server.Domain.Combat;
using StarConflictsRevolt.Server.Domain.Fleets;
using StarConflictsRevolt.Server.Domain.Planets;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Combat;

public interface ICombatSimulator
{
    Task<CombatResult> SimulateFleetCombatAsync(Fleet attacker, Fleet defender, Planet? location = null);
    Task<CombatResult> SimulatePlanetaryCombatAsync(Fleet attacker, Planet defender);
    Task<CombatResult> SimulateDeathStarRunAsync(Fleet attacker, DeathStar defender);
    Task<CombatResult> SimulateMissionAsync(Mission mission, Character agent, Planet target);
}

// Additional interfaces for modular components