using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Keyboards;
using Bliss.CSharp.Interact.Mice;
using StarConflictsRevolt.Clients.Bliss.Core.UI.Interfaces;

namespace StarConflictsRevolt.Clients.Bliss.Core.UI;

/// <summary>
/// Handles input processing for UI components.
/// Follows the Adapter pattern by adapting Bliss input functionality to our interface.
/// </summary>
public class InputHandler : IInputHandler
{
    private readonly Dictionary<KeyboardKey, bool> _previousKeyStates = new();
    private readonly Dictionary<KeyboardKey, bool> _currentKeyStates = new();
    
    public bool IsKeyPressed(KeyboardKey key)
    {
        return Input.IsKeyPressed(key);
    }
    
    public bool IsKeyDown(KeyboardKey key)
    {
        return Input.IsKeyDown(key);
    }
    
    public bool IsKeyReleased(KeyboardKey key)
    {
        // Check if key was down in previous frame but not in current frame
        if (_previousKeyStates.TryGetValue(key, out var wasDown) && wasDown)
        {
            if (_currentKeyStates.TryGetValue(key, out var isDown))
            {
                return !isDown;
            }
            return true;
        }
        return false;
    }
    
    public System.Numerics.Vector2 GetMousePosition()
    {
        return Input.GetMousePosition();
    }
    
    public bool IsLeftMousePressed()
    {
        return Input.IsMouseButtonPressed(MouseButton.Left);
    }
    
    public bool IsRightMousePressed()
    {
        return Input.IsMouseButtonPressed(MouseButton.Right);
    }
    
    public void Update()
    {
        // Update key states for release detection
        _previousKeyStates.Clear();
        foreach (var kvp in _currentKeyStates)
        {
            _previousKeyStates[kvp.Key] = kvp.Value;
        }
        
        _currentKeyStates.Clear();
        
        // Update current key states for all keys we care about
        var keysToTrack = new[]
        {
            KeyboardKey.Up, KeyboardKey.Down, KeyboardKey.Left, KeyboardKey.Right,
            KeyboardKey.Enter, KeyboardKey.Escape, KeyboardKey.Tab,
            KeyboardKey.F1, KeyboardKey.F2, KeyboardKey.F3, KeyboardKey.F12,
            KeyboardKey.ShiftLeft, KeyboardKey.ControlLeft
        };
        
        foreach (var key in keysToTrack)
        {
            _currentKeyStates[key] = Input.IsKeyDown(key);
        }
    }
} 