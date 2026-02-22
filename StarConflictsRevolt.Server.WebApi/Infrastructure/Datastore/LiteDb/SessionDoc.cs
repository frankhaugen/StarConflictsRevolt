using StarConflictsRevolt.Server.WebApi.Core.Domain.Sessions;
using Session = StarConflictsRevolt.Server.WebApi.Core.Domain.Gameplay.Session;

namespace StarConflictsRevolt.Server.WebApi.Infrastructure.Datastore.LiteDb;

internal sealed class SessionDoc
{
    public Guid Id { get; set; }
    public string SessionName { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public bool IsActive { get; set; }
    public DateTime? Ended { get; set; }
    public SessionType SessionType { get; set; }
    public string? ClientId { get; set; }
    public string? PlayerId { get; set; }

    public static SessionDoc From(Session s)
    {
        return new SessionDoc
        {
            Id = s.Id,
            SessionName = s.SessionName,
            Created = s.Created,
            IsActive = s.IsActive,
            Ended = s.Ended,
            SessionType = s.SessionType,
            ClientId = s.ClientId,
            PlayerId = s.PlayerId
        };
    }

    public Session ToSession()
    {
        return new Session
        {
            Id = Id,
            SessionName = SessionName,
            Created = Created,
            IsActive = IsActive,
            Ended = Ended,
            SessionType = SessionType,
            ClientId = ClientId,
            PlayerId = PlayerId
        };
    }
}
