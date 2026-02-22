using StarConflictsRevolt.Server.Domain.Combat;

namespace StarConflictsRevolt.Server.Combat;

public interface ITargetSelector
{
    CombatShip? SelectTarget(CombatShip attacker, List<CombatShip> enemies, CombatState state);
}