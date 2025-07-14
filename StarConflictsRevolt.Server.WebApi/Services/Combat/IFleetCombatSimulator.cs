using StarConflictsRevolt.Server.WebApi.Models;
using StarConflictsRevolt.Server.WebApi.Models.Combat;

namespace StarConflictsRevolt.Server.WebApi.Services.Combat;

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