using StarConflictsRevolt.Server.WebApi.Models.Combat;

namespace StarConflictsRevolt.Server.WebApi.Services.Combat;

public interface ITargetSelector
{
    CombatShip? SelectTarget(CombatShip attacker, List<CombatShip> enemies, CombatState state);
}