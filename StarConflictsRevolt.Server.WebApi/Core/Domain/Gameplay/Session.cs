using StarConflictsRevolt.Server.WebApi.Models;

namespace StarConflictsRevolt.Server.WebApi.Datastore.Entities;

public class Session : IGameObject
{
    public string SessionName { get; set; } = string.Empty;

    public DateTime Created { get; set; }

    public bool IsActive { get; set; }

    public DateTime? Ended { get; set; }

    public SessionType SessionType { get; set; }

    public string? ClientId { get; set; }
    public Client? Client { get; set; }

    /// <inheritdoc />
    public Guid Id { get; set; }
}