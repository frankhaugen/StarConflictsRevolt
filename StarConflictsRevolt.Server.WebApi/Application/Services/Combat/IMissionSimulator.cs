using StarConflictsRevolt.Server.WebApi.Core.Domain.Combat;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Planets;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Combat;

public interface IMissionSimulator
{
    CombatResult SimulateMission(Mission mission, Character agent, Planet target);
    double CalculateMissionDifficulty(Mission mission, Planet target, Character agent);
    double CalculateSkillBonus(Character agent, MissionType missionType);
    double CalculateEnvironmentalModifier(Planet target);
    double CalculateSuccessChance(int difficulty, double skillBonus, double environmentalModifier);
    List<MissionReward> CalculateRewards(Mission mission, bool success, Character agent);
    List<MissionConsequence> ApplyMissionConsequences(Mission mission, bool success, Planet target);
}