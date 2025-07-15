using StarConflictsRevolt.Clients.Bliss.Core.UI.Interfaces;

namespace StarConflictsRevolt.Clients.Bliss.Core.UI.Interfaces;

/// <summary>
/// Factory for creating UI screens with proper dependency injection.
/// Follows the Factory pattern and Dependency Inversion Principle.
/// </summary>
public interface IScreenFactory
{
    /// <summary>
    /// Creates a screen by its ID.
    /// </summary>
    /// <param name="screenId">The ID of the screen to create.</param>
    /// <returns>The created screen, or null if the screen ID is not recognized.</returns>
    IScreen? CreateScreen(string screenId);
    
    /// <summary>
    /// Gets all available screen IDs that can be created by this factory.
    /// </summary>
    /// <returns>An enumerable of available screen IDs.</returns>
    IEnumerable<string> GetAvailableScreenIds();
} 