using StarConflictsRevolt.Server.Domain.Combat;

namespace StarConflictsRevolt.Server.Combat;

public interface IAttackResolver
{
    AttackResult ResolveAttack(CombatShip attacker, CombatShip target, CombatState state);
    double CalculateHitChance(CombatShip attacker, CombatShip target, CombatState state);
    double CalculateDamageModifiers(CombatShip attacker, CombatShip target, CombatState state);
}