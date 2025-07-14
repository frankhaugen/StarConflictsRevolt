namespace StarConflictsRevolt.Server.WebApi.Core.Domain.AI;

public enum GoalTimeframe
{
    Immediate, // This turn
    ShortTerm, // Next 5 turns
    MediumTerm, // Next 20 turns
    LongTerm // Next 100 turns
}