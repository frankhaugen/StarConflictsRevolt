namespace StarConflictsRevolt.Clients.Bliss.Core.UI.Interfaces;

/// <summary>
/// Provides access to user profile information.
/// Follows the Dependency Inversion Principle by depending on abstractions rather than concrete implementations.
/// </summary>
public interface IUserProfileProvider
{
    /// <summary>
    /// Gets the current user's name.
    /// </summary>
    /// <returns>The user's name, or null if not available.</returns>
    string? GetUserName();
    
    /// <summary>
    /// Sets the current user's name.
    /// </summary>
    /// <param name="name">The name to set.</param>
    void SetUserName(string name);
    
    /// <summary>
    /// Gets whether a user profile exists.
    /// </summary>
    /// <returns>True if a user profile exists, false otherwise.</returns>
    bool HasUserProfile();
    
    /// <summary>
    /// Creates a new user profile.
    /// </summary>
    /// <param name="name">The user's name.</param>
    void CreateUserProfile(string name);
    
    /// <summary>
    /// Saves the current user profile.
    /// </summary>
    void SaveUserProfile();
    
    /// <summary>
    /// Loads the user profile from storage.
    /// </summary>
    void LoadUserProfile();
} 