using StarConflictsRevolt.Server.WebApi.Core.Domain.Combat;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Planets;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Combat;

public interface IMissionSimulator
{
    CombatResult SimulateMission(StarConflictsRevolt.Server.WebApi.Core.Domain.Combat.Mission mission, StarConflictsRevolt.Server.WebApi.Core.Domain.Combat.Character agent, Planet target);
    double CalculateMissionDifficulty(StarConflictsRevolt.Server.WebApi.Core.Domain.Combat.Mission mission, StarConflictsRevolt.Server.WebApi.Core.Domain.Combat.Character agent, Planet target);
    double CalculateSkillBonus(StarConflictsRevolt.Server.WebApi.Core.Domain.Combat.Character agent, StarConflictsRevolt.Server.WebApi.Core.Domain.Combat.MissionType missionType);
    double CalculateEnvironmentalModifier(Planet target);
    double CalculateSuccessChance(int difficulty, double skillBonus, double environmentalModifier);
    List<StarConflictsRevolt.Server.WebApi.Core.Domain.Combat.MissionReward> CalculateRewards(StarConflictsRevolt.Server.WebApi.Core.Domain.Combat.Mission mission, bool success, StarConflictsRevolt.Server.WebApi.Core.Domain.Combat.Character agent);
    List<StarConflictsRevolt.Server.WebApi.Core.Domain.Combat.MissionConsequence> ApplyMissionConsequences(StarConflictsRevolt.Server.WebApi.Core.Domain.Combat.Mission mission, bool success, Planet target);
}