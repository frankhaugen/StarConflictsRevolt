namespace StarConflictsRevolt.Server.Core.Models;

public record Session(
    Guid Id,
    string SessionName,
    DateTime Created,
    bool IsActive,
    DateTime? Ended
)
{
    public static Session Create(string sessionName)
    {
        return new Session(Guid.CreateVersion7(), sessionName, DateTime.UtcNow, true, null);
    }
};