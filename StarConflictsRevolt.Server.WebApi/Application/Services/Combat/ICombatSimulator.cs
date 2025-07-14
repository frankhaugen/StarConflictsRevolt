using StarConflictsRevolt.Server.WebApi.Core.Domain.Combat;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Fleets;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Planets;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Combat;

public interface ICombatSimulator
{
    Task<CombatResult> SimulateFleetCombatAsync(Fleet attacker, Fleet defender, Planet? location = null);
    Task<CombatResult> SimulatePlanetaryCombatAsync(Fleet attacker, Planet defender);
    Task<CombatResult> SimulateDeathStarRunAsync(Fleet attacker, DeathStar defender);
    Task<CombatResult> SimulateMissionAsync(Mission mission, Character agent, Planet target);
}

// Additional interfaces for modular components