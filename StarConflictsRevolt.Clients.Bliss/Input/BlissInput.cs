using System.Numerics;
using StarConflictsRevolt.Clients.Bliss.Core;

namespace StarConflictsRevolt.Clients.Bliss.Input;

/// <summary>
/// Mock Bliss implementation of the input interface.
/// This will be replaced with actual Bliss types when the library is properly integrated.
/// </summary>
public class BlissInput : IInput
{
    private readonly Dictionary<Key, bool> _previousKeyStates = new();
    private readonly Dictionary<MouseButton, bool> _previousMouseStates = new();
    private Vector2 _mousePosition = Vector2.Zero;

    public Vector2 MousePosition => _mousePosition;

    public BlissInput(object window) // Mock window parameter
    {
        InitializeKeyStates();
    }

    public bool IsMouseButtonPressed(MouseButton button)
    {
        // Mock: Return false for all mouse buttons
        return false;
    }

    public bool IsMouseButtonJustPressed(MouseButton button)
    {
        var currentState = IsMouseButtonPressed(button);
        var previousState = _previousMouseStates.GetValueOrDefault(button, false);
        return currentState && !previousState;
    }

    public bool IsKeyPressed(Key key)
    {
        // Mock: Return false for all keys
        return false;
    }

    public bool IsKeyJustPressed(Key key)
    {
        var currentState = IsKeyPressed(key);
        var previousState = _previousKeyStates.GetValueOrDefault(key, false);
        return currentState && !previousState;
    }

    public void Update()
    {
        // Update previous key states
        foreach (var key in Enum.GetValues<Key>())
        {
            _previousKeyStates[key] = IsKeyPressed(key);
        }

        // Update previous mouse states
        foreach (var button in Enum.GetValues<MouseButton>())
        {
            _previousMouseStates[button] = IsMouseButtonPressed(button);
        }
    }

    private void InitializeKeyStates()
    {
        foreach (var key in Enum.GetValues<Key>())
        {
            _previousKeyStates[key] = false;
        }

        foreach (var button in Enum.GetValues<MouseButton>())
        {
            _previousMouseStates[button] = false;
        }
    }
} 