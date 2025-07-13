namespace StarConflictsRevolt.Server.WebApi.Models;

public record Session(
    Guid Id,
    string SessionName,
    DateTime Created,
    bool IsActive,
    DateTime? Ended,
    SessionType SessionType
)
{
    public static Session Create(string sessionName, SessionType sessionType)
    {
        return new Session(Guid.CreateVersion7(), sessionName, DateTime.UtcNow, true, null, sessionType);
    }
};