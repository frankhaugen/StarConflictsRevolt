using StarConflictsRevolt.Clients.Raylib.Core;

namespace StarConflictsRevolt.Clients.Raylib.Rendering.Core;

/// <summary>
/// Represents a view in the game UI system.
/// </summary>
public interface IView
{
    /// <summary>
    /// The type of view this represents.
    /// </summary>
    GameView ViewType { get; }
    
    /// <summary>
    /// Draw the view content.
    /// </summary>
    void Draw();
}