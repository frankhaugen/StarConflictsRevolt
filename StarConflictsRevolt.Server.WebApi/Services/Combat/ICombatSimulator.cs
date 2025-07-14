using StarConflictsRevolt.Server.WebApi.Models;
using StarConflictsRevolt.Server.WebApi.Models.Combat;

namespace StarConflictsRevolt.Server.WebApi.Services.Combat;

public interface ICombatSimulator
{
    Task<CombatResult> SimulateFleetCombatAsync(Fleet attacker, Fleet defender, Planet? location = null);
    Task<CombatResult> SimulatePlanetaryCombatAsync(Fleet attacker, Planet defender);
    Task<CombatResult> SimulateDeathStarRunAsync(Fleet attacker, DeathStar defender);
    Task<CombatResult> SimulateMissionAsync(Mission mission, Character agent, Planet target);
}

public interface IFleetCombatSimulator
{
    CombatResult SimulateFleetCombat(Fleet attacker, Fleet defender, Planet? location = null);
    CombatState InitializeCombat(Fleet attacker, Fleet defender, Planet? location = null);
    List<CombatShip> ConvertFleetToCombatShips(Fleet fleet, bool isAttacker);
    List<CombatShip> DetermineInitiativeOrder(List<CombatShip> ships);
    CombatShip? SelectTarget(CombatShip attacker, List<CombatShip> enemies, CombatState state);
    AttackResult ResolveAttack(CombatShip attacker, CombatShip target, CombatState state);
    void ApplyDamage(AttackResult attackResult, CombatShip target);
    bool CheckCombatEnd(CombatState state);
    CombatResult FinalizeCombat(CombatState state);
}

public interface IPlanetaryCombatSimulator
{
    CombatResult SimulatePlanetaryCombat(Fleet attacker, Planet defender);
    BombardmentResult ResolveOrbitalBombardment(Fleet attacker, Planet defender);
    GroundCombatResult ResolveGroundCombat(GroundCombat combat, Planet planet);
    CaptureResult DeterminePlanetaryCapture(GroundCombatResult combat, Planet planet);
}

public interface IDeathStarRunSimulator
{
    CombatResult SimulateDeathStarRun(Fleet attacker, DeathStar defender);
    bool ResolveApproachPhase(Fleet attacker, DeathStar defender, DeathStarRunState state);
    bool ResolveTrenchEntry(Fleet attacker, DeathStar defender, DeathStarRunState state);
    TrenchRunResult ResolveTrenchRun(Fleet attacker, DeathStar defender, DeathStarRunState state);
    CombatResult ResolveExhaustPortAttack(Fleet attacker, DeathStar defender, DeathStarRunState state);
}

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

// Additional interfaces for modular components
public interface ITargetSelector
{
    CombatShip? SelectTarget(CombatShip attacker, List<CombatShip> enemies, CombatState state);
}

public interface IAttackResolver
{
    AttackResult ResolveAttack(CombatShip attacker, CombatShip target, CombatState state);
    double CalculateHitChance(CombatShip attacker, CombatShip target, CombatState state);
    double CalculateDamageModifiers(CombatShip attacker, CombatShip target, CombatState state);
}

public interface ICombatEndChecker
{
    bool CheckCombatEnd(CombatState state);
    string? GetEndReason(CombatState state);
}

public interface ICombatResultCalculator
{
    CombatResult CalculateResult(CombatState state);
    List<CombatReward> CalculateRewards(CombatState state);
    List<CombatConsequence> CalculateConsequences(CombatState state);
    CombatCinematicData GenerateCinematicData(CombatState state);
} 