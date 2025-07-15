namespace StarConflictsRevolt.Clients.Shared.Configuration;

/// <summary>
/// Generic interface for client initializer that can be used by both Raylib and Bliss clients.
/// This removes client-specific dependencies.
/// </summary>
public interface IClientInitializer
{
    /// <summary>
    /// Initializes the client with configuration and authentication
    /// </summary>
    Task InitializeAsync();
}

/// <summary>
/// Generic client context interface that can be implemented by different clients
/// </summary>
public interface IClientContext
{
    /// <summary>
    /// Client identifier
    /// </summary>
    string ClientId { get; set; }

    /// <summary>
    /// Player name
    /// </summary>
    string PlayerName { get; set; }

    /// <summary>
    /// Player identifier
    /// </summary>
    string PlayerId { get; set; }

    /// <summary>
    /// Access token for authentication
    /// </summary>
    string? AccessToken { get; set; }
} 