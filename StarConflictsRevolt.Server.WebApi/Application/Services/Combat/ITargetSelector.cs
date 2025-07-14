using StarConflictsRevolt.Server.WebApi.Core.Domain.Combat;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Combat;

public interface ITargetSelector
{
    CombatShip? SelectTarget(CombatShip attacker, List<CombatShip> enemies, CombatState state);
}