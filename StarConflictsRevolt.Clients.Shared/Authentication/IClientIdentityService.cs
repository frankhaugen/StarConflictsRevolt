namespace StarConflictsRevolt.Clients.Shared.Authentication;

/// <summary>
/// Generic interface for client identity service that can be used by both Raylib and Bliss clients.
/// This removes platform-specific dependencies.
/// </summary>
public interface IClientIdentityService
{
    /// <summary>
    /// Gets or creates a unique client identifier
    /// </summary>
    /// <returns>Unique client ID</returns>
    string GetOrCreateClientId();

    /// <summary>
    /// Gets the user profile information
    /// </summary>
    /// <returns>User profile information</returns>
    IUserProfile GetUserProfile();
}

/// <summary>
/// Generic user profile interface that can be implemented by different platforms
/// </summary>
public interface IUserProfile
{
    /// <summary>
    /// Unique user identifier
    /// </summary>
    string UserId { get; }

    /// <summary>
    /// User's display name
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// User's username
    /// </summary>
    string UserName { get; }
} 