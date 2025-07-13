using StarConflictsRevolt.Server.WebApi.Datastore.Entities;

namespace StarConflictsRevolt.Server.WebApi.Datastore;

public class Client
{
    public string Id { get; set; }
    public DateTime LastSeen { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Session> Sessions { get; set; }
}