using StarConflictsRevolt.Server.Domain.Combat;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Combat;

public interface ICombatEndChecker
{
    bool CheckCombatEnd(CombatState state);
    string? GetEndReason(CombatState state);
}