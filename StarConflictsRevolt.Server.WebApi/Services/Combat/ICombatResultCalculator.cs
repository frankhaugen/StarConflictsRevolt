using StarConflictsRevolt.Server.WebApi.Models.Combat;

namespace StarConflictsRevolt.Server.WebApi.Services.Combat;

public interface ICombatResultCalculator
{
    CombatResult CalculateResult(CombatState state);
    List<CombatReward> CalculateRewards(CombatState state);
    List<CombatConsequence> CalculateConsequences(CombatState state);
    CombatCinematicData GenerateCinematicData(CombatState state);
}