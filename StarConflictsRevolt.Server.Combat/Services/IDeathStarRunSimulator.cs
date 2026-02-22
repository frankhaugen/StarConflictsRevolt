using StarConflictsRevolt.Server.Domain.Combat;
using StarConflictsRevolt.Server.Domain.Fleets;

namespace StarConflictsRevolt.Server.Combat;

public interface IDeathStarRunSimulator
{
    CombatResult SimulateDeathStarRun(Fleet attacker, DeathStar defender);
    bool ResolveApproachPhase(Fleet attacker, DeathStar defender, DeathStarRunState state);
    bool ResolveTrenchEntry(Fleet attacker, DeathStar defender, DeathStarRunState state);
    TrenchRunResult ResolveTrenchRun(Fleet attacker, DeathStar defender, DeathStarRunState state);
    CombatResult ResolveExhaustPortAttack(Fleet attacker, DeathStar defender, DeathStarRunState state);
}