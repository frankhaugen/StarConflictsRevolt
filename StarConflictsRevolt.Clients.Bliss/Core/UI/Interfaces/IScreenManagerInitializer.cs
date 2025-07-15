namespace StarConflictsRevolt.Clients.Bliss.Core.UI.Interfaces;

/// <summary>
/// Initializes the screen manager with all available screens.
/// Follows the Single Responsibility Principle by focusing only on initialization.
/// </summary>
public interface IScreenManagerInitializer
{
    /// <summary>
    /// Initializes the screen manager by creating and registering all available screens.
    /// </summary>
    void Initialize();
} 