namespace StarConflictsRevolt.Clients.Shared.Authentication;

/// <summary>
/// Ambient context for the client id to use when requesting an auth token.
/// When set (e.g. by Blazor before API calls), the token provider uses this
/// instead of the static config ClientId so the same identity is used for token and game session.
/// </summary>
public static class AuthClientIdContext
{
    private static readonly AsyncLocal<string?> CurrentClientId = new();

    /// <summary>
    /// Gets or sets the client id for the current async context. When set, token requests use this id.
    /// </summary>
    public static string? Current
    {
        get => CurrentClientId.Value;
        set => CurrentClientId.Value = value;
    }
}
