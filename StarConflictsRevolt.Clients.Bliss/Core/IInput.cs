using System.Numerics;

namespace StarConflictsRevolt.Clients.Bliss.Core;

/// <summary>
/// Abstracts keyboard/mouse/gamepad input from Bliss's SDL3 wrapper.
/// </summary>
public interface IInput
{
    /// <summary>
    /// Gets the current mouse position.
    /// </summary>
    Vector2 MousePosition { get; }
    
    /// <summary>
    /// Gets whether the specified mouse button is currently pressed.
    /// </summary>
    /// <param name="button">The mouse button to check</param>
    /// <returns>True if the button is pressed</returns>
    bool IsMouseButtonPressed(MouseButton button);
    
    /// <summary>
    /// Gets whether the specified mouse button was just pressed this frame.
    /// </summary>
    /// <param name="button">The mouse button to check</param>
    /// <returns>True if the button was just pressed</returns>
    bool IsMouseButtonJustPressed(MouseButton button);
    
    /// <summary>
    /// Gets whether the specified key is currently pressed.
    /// </summary>
    /// <param name="key">The key to check</param>
    /// <returns>True if the key is pressed</returns>
    bool IsKeyPressed(Key key);
    
    /// <summary>
    /// Gets whether the specified key was just pressed this frame.
    /// </summary>
    /// <param name="key">The key to check</param>
    /// <returns>True if the key was just pressed</returns>
    bool IsKeyJustPressed(Key key);
    
    /// <summary>
    /// Updates the input state for the current frame.
    /// </summary>
    void Update();
}

/// <summary>
/// Represents mouse buttons.
/// </summary>
public enum MouseButton
{
    Left,
    Right,
    Middle
}

/// <summary>
/// Represents keyboard keys.
/// </summary>
public enum Key
{
    // Navigation keys
    Escape,
    Enter,
    Space,
    Tab,
    
    // Function keys
    F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12,
    
    // Arrow keys
    Up, Down, Left, Right,
    
    // Letter keys
    A, B, C, D, E, F, G, H, I, J, K, L, M,
    N, O, P, Q, R, S, T, U, V, W, X, Y, Z,
    
    // Number keys
    D0, D1, D2, D3, D4, D5, D6, D7, D8, D9,
    
    // Other keys
    Backspace,
    Delete,
    Home,
    End,
    PageUp,
    PageDown
} 