using StarConflictsRevolt.Server.WebApi.Core.Domain.Combat;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Combat;

public interface IAttackResolver
{
    AttackResult ResolveAttack(CombatShip attacker, CombatShip target, CombatState state);
    double CalculateHitChance(CombatShip attacker, CombatShip target, CombatState state);
    double CalculateDamageModifiers(CombatShip attacker, CombatShip target, CombatState state);
}