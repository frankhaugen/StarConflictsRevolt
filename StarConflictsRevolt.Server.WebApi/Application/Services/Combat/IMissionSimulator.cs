using StarConflictsRevolt.Server.WebApi.Models;
using StarConflictsRevolt.Server.WebApi.Models.Combat;

namespace StarConflictsRevolt.Server.WebApi.Services.Combat;

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