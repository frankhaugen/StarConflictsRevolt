using StarConflictsRevolt.Server.WebApi.Core.Domain.Combat;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Fleets;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Combat;

public interface IDeathStarRunSimulator
{
    CombatResult SimulateDeathStarRun(Fleet attacker, DeathStar defender);
    bool ResolveApproachPhase(Fleet attacker, DeathStar defender, DeathStarRunState state);
    bool ResolveTrenchEntry(Fleet attacker, DeathStar defender, DeathStarRunState state);
    TrenchRunResult ResolveTrenchRun(Fleet attacker, DeathStar defender, DeathStarRunState state);
    CombatResult ResolveExhaustPortAttack(Fleet attacker, DeathStar defender, DeathStarRunState state);
}