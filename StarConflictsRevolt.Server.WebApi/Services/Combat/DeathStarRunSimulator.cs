using StarConflictsRevolt.Server.WebApi.Models;
using StarConflictsRevolt.Server.WebApi.Models.Combat;

namespace StarConflictsRevolt.Server.WebApi.Services.Combat;

public class DeathStarRunSimulator : IDeathStarRunSimulator
{
    /// <inheritdoc />
    public CombatResult SimulateDeathStarRun(Fleet attacker, DeathStar defender)
    {
        throw new NotImplementedException("Death Star run simulation is not implemented yet.");
    }

    /// <inheritdoc />
    public bool ResolveApproachPhase(Fleet attacker, DeathStar defender, DeathStarRunState state)
    {
        throw new NotImplementedException("Death Star run simulation is not implemented yet.");
    }

    /// <inheritdoc />
    public bool ResolveTrenchEntry(Fleet attacker, DeathStar defender, DeathStarRunState state)
    {
        throw new NotImplementedException("Death Star run simulation is not implemented yet.");
    }

    /// <inheritdoc />
    public TrenchRunResult ResolveTrenchRun(Fleet attacker, DeathStar defender, DeathStarRunState state)
    {
        throw new NotImplementedException("Death Star run simulation is not implemented yet.");
    }

    /// <inheritdoc />
    public CombatResult ResolveExhaustPortAttack(Fleet attacker, DeathStar defender, DeathStarRunState state)
    {
        throw new NotImplementedException("Death Star run simulation is not implemented yet.");
    }
}