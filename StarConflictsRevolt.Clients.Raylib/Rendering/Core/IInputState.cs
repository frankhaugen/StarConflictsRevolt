using System.Numerics;
using Raylib_CSharp.Interact;

namespace StarConflictsRevolt.Clients.Raylib.Rendering.Core;

/// <summary>
/// Abstract input state interface for testable input handling.
/// </summary>
public interface IInputState
{
    /// <summary>
    /// Current mouse position.
    /// </summary>
    Vector2 MousePosition { get; }
    
    /// <summary>
    /// Mouse wheel movement.
    /// </summary>
    float MouseWheelMove { get; }
    
    /// <summary>
    /// Check if a key is currently pressed.
    /// </summary>
    /// <param name="key">Key to check.</param>
    /// <returns>True if the key is pressed.</returns>
    bool IsKeyDown(KeyboardKey key);
    
    /// <summary>
    /// Check if a key was pressed this frame.
    /// </summary>
    /// <param name="key">Key to check.</param>
    /// <returns>True if the key was pressed this frame.</returns>
    bool IsKeyPressed(KeyboardKey key);
    
    /// <summary>
    /// Check if a key was released this frame.
    /// </summary>
    /// <param name="key">Key to check.</param>
    /// <returns>True if the key was released this frame.</returns>
    bool IsKeyReleased(KeyboardKey key);
    
    /// <summary>
    /// Check if a mouse button is currently pressed.
    /// </summary>
    /// <param name="button">Button to check.</param>
    /// <returns>True if the button is pressed.</returns>
    bool IsMouseButtonDown(MouseButton button);
    
    /// <summary>
    /// Check if a mouse button was pressed this frame.
    /// </summary>
    /// <param name="button">Button to check.</param>
    /// <returns>True if the button was pressed this frame.</returns>
    bool IsMouseButtonPressed(MouseButton button);
    
    /// <summary>
    /// Check if a mouse button was released this frame.
    /// </summary>
    /// <param name="button">Button to check.</param>
    /// <returns>True if the button was released this frame.</returns>
    bool IsMouseButtonReleased(MouseButton button);
    
    /// <summary>
    /// Get the character pressed this frame.
    /// </summary>
    /// <returns>Character code or 0 if no character was pressed.</returns>
    int GetCharPressed();
} 