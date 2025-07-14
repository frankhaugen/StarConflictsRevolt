using StarConflictsRevolt.Server.WebApi.Models.Combat;

namespace StarConflictsRevolt.Server.WebApi.Services.Combat;

public interface IAttackResolver
{
    AttackResult ResolveAttack(CombatShip attacker, CombatShip target, CombatState state);
    double CalculateHitChance(CombatShip attacker, CombatShip target, CombatState state);
    double CalculateDamageModifiers(CombatShip attacker, CombatShip target, CombatState state);
}