namespace StarConflictsRevolt.Clients.Shared.Configuration;

public class ClientContext : IClientContext
{
    /// <inheritdoc />
    public string ClientId { get; set; } = string.Empty;

    /// <inheritdoc />
    public string PlayerName { get; set; } = string.Empty;

    /// <inheritdoc />
    public string PlayerId { get; set; } = string.Empty;

    /// <inheritdoc />
    public string? AccessToken { get; set; }
}