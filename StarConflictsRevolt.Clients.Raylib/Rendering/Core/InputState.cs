using System.Numerics;
using Raylib_CSharp.Interact;

namespace StarConflictsRevolt.Clients.Raylib.Rendering.Core;

/// <summary>
/// Raylib implementation of IInputState.
/// </summary>
public class InputState : IInputState
{
    public Vector2 MousePosition => Input.GetMousePosition();
    public float MouseWheelMove => Input.GetMouseWheelMove();
    
    public bool IsKeyDown(KeyboardKey key) => Input.IsKeyDown(key);
    public bool IsKeyPressed(KeyboardKey key) => Input.IsKeyPressed(key);
    public bool IsKeyReleased(KeyboardKey key) => Input.IsKeyReleased(key);
    
    public bool IsMouseButtonDown(MouseButton button) => Input.IsMouseButtonDown(button);
    public bool IsMouseButtonPressed(MouseButton button) => Input.IsMouseButtonPressed(button);
    public bool IsMouseButtonReleased(MouseButton button) => Input.IsMouseButtonReleased(button);
    
    public int GetCharPressed() => Input.GetCharPressed();
} 