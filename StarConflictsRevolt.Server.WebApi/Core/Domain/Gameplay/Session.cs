using StarConflictsRevolt.Server.WebApi.Core.Domain.Enums;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Sessions;

namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Gameplay;

public class Session : IGameObject
{
    public string SessionName { get; set; } = string.Empty;

    public DateTime Created { get; set; }

    public bool IsActive { get; set; }

    public DateTime? Ended { get; set; }

    public SessionType SessionType { get; set; }

    /// <summary>OAuth client reference (FK to Clients). Use null when creating sessions from game client.</summary>
    public string? ClientId { get; set; }
    public Client? Client { get; set; }

    /// <summary>Player/client tracking id (e.g. from browser localStorage). Not an FK; used for single-player resume.</summary>
    public string? PlayerId { get; set; }

    /// <inheritdoc />
    public Guid Id { get; set; }
}