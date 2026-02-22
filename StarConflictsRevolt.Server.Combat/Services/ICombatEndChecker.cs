using StarConflictsRevolt.Server.Domain.Combat;

namespace StarConflictsRevolt.Server.Combat;

public interface ICombatEndChecker
{
    bool CheckCombatEnd(CombatState state);
    string? GetEndReason(CombatState state);
}