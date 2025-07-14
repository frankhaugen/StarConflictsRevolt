using StarConflictsRevolt.Server.WebApi.Models.Combat;

namespace StarConflictsRevolt.Server.WebApi.Services.Combat;

public interface ICombatEndChecker
{
    bool CheckCombatEnd(CombatState state);
    string? GetEndReason(CombatState state);
}