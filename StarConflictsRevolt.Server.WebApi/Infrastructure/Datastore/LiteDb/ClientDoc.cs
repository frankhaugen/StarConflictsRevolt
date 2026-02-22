using StarConflictsRevolt.Server.WebApi.Core.Domain.Gameplay;

namespace StarConflictsRevolt.Server.WebApi.Infrastructure.Datastore.LiteDb;

internal sealed class ClientDoc
{
    public string Id { get; set; } = string.Empty;
    public DateTime LastSeen { get; set; }
    public bool IsActive { get; set; } = true;

    public static ClientDoc From(Client c)
    {
        return new ClientDoc { Id = c.Id, LastSeen = c.LastSeen, IsActive = c.IsActive };
    }

    public Client ToClient()
    {
        return new Client { Id = Id, LastSeen = LastSeen, IsActive = IsActive };
    }
}
