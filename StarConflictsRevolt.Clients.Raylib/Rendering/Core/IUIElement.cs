using System.Numerics;

namespace StarConflictsRevolt.Clients.Raylib.Rendering.Core;

/// <summary>
/// Represents a single UI element that can be tested independently of rendering.
/// </summary>
public interface IUIElement
{
    /// <summary>
    /// Unique identifier for this element.
    /// </summary>
    string Id { get; }
    
    /// <summary>
    /// Position of the element.
    /// </summary>
    Vector2 Position { get; set; }
    
    /// <summary>
    /// Size of the element.
    /// </summary>
    Vector2 Size { get; set; }
    
    /// <summary>
    /// Whether the element is visible.
    /// </summary>
    bool IsVisible { get; set; }
    
    /// <summary>
    /// Whether the element is enabled for interaction.
    /// </summary>
    bool IsEnabled { get; set; }
    
    /// <summary>
    /// Whether the element has focus.
    /// </summary>
    bool HasFocus { get; set; }
    
    /// <summary>
    /// Bounds of the element for hit testing.
    /// </summary>
    Rectangle Bounds { get; }
    
    /// <summary>
    /// Update the element logic.
    /// </summary>
    /// <param name="deltaTime">Time since last update.</param>
    /// <param name="inputState">Current input state.</param>
    void Update(float deltaTime, IInputState inputState);
    
    /// <summary>
    /// Render the element.
    /// </summary>
    /// <param name="renderer">Renderer to use.</param>
    void Render(IUIRenderer renderer);
    
    /// <summary>
    /// Handle input events for this element.
    /// </summary>
    /// <param name="inputState">Current input state.</param>
    /// <returns>True if the element handled the input.</returns>
    bool HandleInput(IInputState inputState);
    
    /// <summary>
    /// Check if a point is within this element's bounds.
    /// </summary>
    /// <param name="point">Point to test.</param>
    /// <returns>True if the point is within bounds.</returns>
    bool Contains(Vector2 point);
}

/// <summary>
/// Rectangle representation for bounds checking.
/// </summary>
public struct Rectangle
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    
    public Rectangle(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
    
    public bool Contains(Vector2 point)
    {
        return point.X >= X && point.X <= X + Width && 
               point.Y >= Y && point.Y <= Y + Height;
    }
    
    public static Rectangle FromPositionAndSize(Vector2 position, Vector2 size)
    {
        return new Rectangle(position.X, position.Y, size.X, size.Y);
    }
} 