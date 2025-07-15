using Bliss.CSharp.Interact.Keyboards;

namespace StarConflictsRevolt.Clients.Bliss.Core.UI.Interfaces;

/// <summary>
/// Handles input processing for UI components.
/// Follows the Interface Segregation Principle by providing focused input handling capabilities.
/// </summary>
public interface IInputHandler
{
    /// <summary>
    /// Gets whether a key is currently pressed.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>True if the key is pressed, false otherwise.</returns>
    bool IsKeyPressed(KeyboardKey key);
    
    /// <summary>
    /// Gets whether a key is currently down.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>True if the key is down, false otherwise.</returns>
    bool IsKeyDown(KeyboardKey key);
    
    /// <summary>
    /// Gets whether a key was just released.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>True if the key was just released, false otherwise.</returns>
    bool IsKeyReleased(KeyboardKey key);
    
    /// <summary>
    /// Gets the current mouse position.
    /// </summary>
    /// <returns>The current mouse position as a Vector2.</returns>
    System.Numerics.Vector2 GetMousePosition();
    
    /// <summary>
    /// Gets whether the left mouse button is pressed.
    /// </summary>
    /// <returns>True if the left mouse button is pressed, false otherwise.</returns>
    bool IsLeftMousePressed();
    
    /// <summary>
    /// Gets whether the right mouse button is pressed.
    /// </summary>
    /// <returns>True if the right mouse button is pressed, false otherwise.</returns>
    bool IsRightMousePressed();
    
    /// <summary>
    /// Updates the input handler state.
    /// </summary>
    void Update();
} 