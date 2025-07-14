using System.Numerics;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using StarConflictsRevolt.Clients.Raylib.Rendering.Core;

namespace StarConflictsRevolt.Clients.Raylib.Rendering.UI;

/// <summary>
/// A clickable button UI element.
/// </summary>
public class ButtonElement : IUIElement
{
    public string Id { get; }
    public bool IsVisible { get; set; } = true;
    public bool IsEnabled { get; set; } = true;
    public bool HasFocus { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }
    public Rectangle Bounds => Rectangle.FromPositionAndSize(Position, Size);
    
    public string Text { get; set; }
    public Color BackgroundColor { get; set; } = Color.Gray;
    public Color TextColor { get; set; } = Color.White;
    public Color HoverColor { get; set; } = Color.LightGray;
    public Color PressedColor { get; set; } = Color.DarkGray;
    
    public event Action? Clicked;
    
    private bool _isHovered;
    private bool _isPressed;
    
    public ButtonElement(string id, string text, Vector2 position, Vector2 size)
    {
        Id = id;
        Text = text;
        Position = position;
        Size = size;
    }
    
    public void Update(float deltaTime, IInputState inputState)
    {
        if (!IsEnabled || !IsVisible) return;
        
        var mousePos = inputState.MousePosition;
        
        // Check if mouse is over button
        _isHovered = Contains(mousePos);
        
        // Check if button is pressed
        if (_isHovered && inputState.IsMouseButtonPressed(MouseButton.Left))
        {
            _isPressed = true;
        }
        else if (inputState.IsMouseButtonReleased(MouseButton.Left))
        {
            if (_isPressed && _isHovered)
            {
                Clicked?.Invoke();
            }
            _isPressed = false;
        }
    }
    
    public void Render(IUIRenderer renderer)
    {
        if (!IsVisible) return;
        
        // Determine button color based on state
        var buttonColor = _isPressed ? PressedColor : _isHovered ? HoverColor : BackgroundColor;
        
        // Draw button background
        renderer.DrawRectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y, buttonColor);
        
        // Draw button text
        if (!string.IsNullOrEmpty(Text))
        {
            var textX = (int)Position.X + 10;
            var textY = (int)Position.Y + ((int)Size.Y - 20) / 2; // Center text vertically
            renderer.DrawText(Text, textX, textY, 20, TextColor);
        }
    }
    
    public bool HandleInput(IInputState inputState)
    {
        if (!IsEnabled || !IsVisible) return false;
        
        // Handle keyboard input for accessibility
        if (HasFocus && inputState.IsKeyPressed(KeyboardKey.Enter))
        {
            Clicked?.Invoke();
            return true;
        }
        
        return false;
    }
    
    public bool Contains(Vector2 point)
    {
        return Bounds.Contains(point);
    }
} 