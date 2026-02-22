using StarConflictsRevolt.Server.Domain.Combat;
using StarConflictsRevolt.Server.Domain.Planets;

namespace StarConflictsRevolt.Server.Combat;

public interface IMissionSimulator
{
    CombatResult SimulateMission(StarConflictsRevolt.Server.Domain.Combat.Mission mission, StarConflictsRevolt.Server.Domain.Combat.Character agent, Planet target);
    double CalculateMissionDifficulty(StarConflictsRevolt.Server.Domain.Combat.Mission mission, StarConflictsRevolt.Server.Domain.Combat.Character agent, Planet target);
    double CalculateSkillBonus(StarConflictsRevolt.Server.Domain.Combat.Character agent, StarConflictsRevolt.Server.Domain.Combat.MissionType missionType);
    double CalculateEnvironmentalModifier(Planet target);
    double CalculateSuccessChance(int difficulty, double skillBonus, double environmentalModifier);
    List<StarConflictsRevolt.Server.Domain.Combat.MissionReward> CalculateRewards(StarConflictsRevolt.Server.Domain.Combat.Mission mission, bool success, StarConflictsRevolt.Server.Domain.Combat.Character agent);
    List<StarConflictsRevolt.Server.Domain.Combat.MissionConsequence> ApplyMissionConsequences(StarConflictsRevolt.Server.Domain.Combat.Mission mission, bool success, Planet target);
}