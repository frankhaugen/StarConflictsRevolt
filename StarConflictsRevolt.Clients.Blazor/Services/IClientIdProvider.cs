namespace StarConflictsRevolt.Clients.Blazor.Services;

/// <summary>
/// Provides a persistent client/player id for this browser (e.g. from localStorage)
/// so the server can resume the same single-player session instead of creating new worlds.
/// </summary>
public interface IClientIdProvider
{
    Task<string> GetClientIdAsync(CancellationToken cancellationToken = default);
}
