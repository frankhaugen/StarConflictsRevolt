using StarConflictsRevolt.Clients.Bliss.Core;

namespace StarConflictsRevolt.Clients.Bliss.Rendering;

/// <summary>
/// Represents a view in the Bliss game UI system.
/// </summary>
public interface IView
{
    /// <summary>
    /// The type of view this represents.
    /// </summary>
    GameView ViewType { get; }
    
    /// <summary>
    /// Updates the view logic for the current frame.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last frame</param>
    void Update(float deltaTime);
    
    /// <summary>
    /// Draws the view content.
    /// </summary>
    void Draw();
    
    /// <summary>
    /// Called when the view becomes active.
    /// </summary>
    void OnActivate();
    
    /// <summary>
    /// Called when the view becomes inactive.
    /// </summary>
    void OnDeactivate();
} 