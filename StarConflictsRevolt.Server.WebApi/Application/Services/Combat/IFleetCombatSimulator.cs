using StarConflictsRevolt.Server.WebApi.Core.Domain.Combat;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Fleets;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Planets;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Combat;

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