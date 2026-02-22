using StarConflictsRevolt.Server.Domain.Gameplay;

namespace StarConflictsRevolt.Server.WebApi.Infrastructure.Datastore.LiteDb;

public class ClientDoc
{
    public string Id { get; set; } = string.Empty;
    public DateTime LastSeen { get; set; }
    public bool IsActive { get; set; } = true;

    public static ClientDoc From(Client c) => new()
    {
        Id = c.Id,
        LastSeen = c.LastSeen,
        IsActive = c.IsActive
    };

    public Client ToClient() => new()
    {
        Id = Id,
        LastSeen = LastSeen,
        IsActive = IsActive
    };
}
