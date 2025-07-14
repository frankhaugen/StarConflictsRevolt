using StarConflictsRevolt.Server.WebApi.Core.Domain.Combat;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Combat;

public interface ICombatResultCalculator
{
    CombatResult CalculateResult(CombatState state);
    List<CombatReward> CalculateRewards(CombatState state);
    List<CombatConsequence> CalculateConsequences(CombatState state);
    CombatCinematicData GenerateCinematicData(CombatState state);
}