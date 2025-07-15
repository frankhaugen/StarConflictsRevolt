using System.Numerics;
using Raylib_CSharp.Colors;
using StarConflictsRevolt.Clients.Raylib.Rendering.UI;

namespace StarConflictsRevolt.Clients.Raylib.Rendering.Core;

/// <summary>
/// Minimap element showing a small overview of the game world.
/// </summary>
public class MinimapElement : IUIElement
{
    private Rectangle _bounds;
    
    public string Id => "Minimap";
    public Vector2 Position { get => new(_bounds.X, _bounds.Y); set => _bounds = new Rectangle(value.X, value.Y, _bounds.Width, _bounds.Height); }
    public Vector2 Size { get => new(_bounds.Width, _bounds.Height); set => _bounds = new Rectangle(_bounds.X, _bounds.Y, value.X, value.Y); }
    public bool IsVisible { get; set; } = true;
    public bool IsEnabled { get; set; } = true;
    public bool HasFocus { get; set; } = false;
    public Rectangle Bounds => _bounds;
    
    public MinimapElement(Rectangle bounds)
    {
        _bounds = bounds;
    }
    
    public void Update(float deltaTime, IInputState inputState) { }
    
    public void Render(IUIRenderer renderer)
    {
        // Draw background
        renderer.DrawRectangle((int)_bounds.X, (int)_bounds.Y, (int)_bounds.Width, (int)_bounds.Height, UIHelper.SciFiColors.MinimapBackground);
        renderer.DrawRectangleLines((int)_bounds.X, (int)_bounds.Y, (int)_bounds.Width, (int)_bounds.Height, UIHelper.SciFiColors.MinimapGrid);
        
        // Draw title
        renderer.DrawText("Minimap", (int)_bounds.X + 5, (int)_bounds.Y + 5, UIHelper.FontSizes.Small, Color.White);
        
        // Draw placeholder content
        renderer.DrawText("World Overview", (int)_bounds.X + 5, (int)_bounds.Y + 25, UIHelper.FontSizes.Small, Color.Gray);
    }
    
    public bool HandleInput(IInputState inputState) => false;
    public bool Contains(Vector2 point) => _bounds.Contains(point);
    
    public void Resize(Rectangle newBounds)
    {
        _bounds = newBounds;
    }
}