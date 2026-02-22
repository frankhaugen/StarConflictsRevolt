namespace StarConflictsRevolt.Clients.Blazor.Services;

/// <summary>
/// Client-side storage for current session id (sessionStorage) and player name (localStorage).
/// Session id is view-independent and tab-scoped; player name persists across browser sessions.
/// </summary>
public interface IClientSessionStorage
{
    /// <summary>Gets the stored session id, or null if none (sessionStorage).</summary>
    Task<Guid?> GetSessionIdAsync(CancellationToken cancellationToken = default);

    /// <summary>Stores the session id; pass null to clear (sessionStorage).</summary>
    Task SetSessionIdAsync(Guid? sessionId, CancellationToken cancellationToken = default);

    /// <summary>Gets the stored player name, or null if none (localStorage).</summary>
    Task<string?> GetPlayerNameAsync(CancellationToken cancellationToken = default);

    /// <summary>Stores the player name; pass null or empty to clear (localStorage).</summary>
    Task SetPlayerNameAsync(string? playerName, CancellationToken cancellationToken = default);
}
