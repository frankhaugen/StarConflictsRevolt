namespace StarConflictsRevolt.Clients.Shared.Configuration;

public class ClientContext : IClientContext
{
    /// <inheritdoc />
    public string ClientId { get; set; }

    /// <inheritdoc />
    public string PlayerName { get; set; }

    /// <inheritdoc />
    public string PlayerId { get; set; }

    /// <inheritdoc />
    public string? AccessToken { get; set; }
}