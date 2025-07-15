using System.Numerics;
using Raylib_CSharp.Colors;
using StarConflictsRevolt.Clients.Raylib.Rendering.UI;

namespace StarConflictsRevolt.Clients.Raylib.Rendering.Core;

/// <summary>
/// Side panel element showing object information and actions.
/// </summary>
public class SidePanelElement : IUIElement
{
    private Rectangle _bounds;
    
    public string Id => "SidePanel";
    public Vector2 Position { get => new(_bounds.X, _bounds.Y); set => _bounds = new Rectangle(value.X, value.Y, _bounds.Width, _bounds.Height); }
    public Vector2 Size { get => new(_bounds.Width, _bounds.Height); set => _bounds = new Rectangle(_bounds.X, _bounds.Y, value.X, value.Y); }
    public bool IsVisible { get; set; } = true;
    public bool IsEnabled { get; set; } = true;
    public bool HasFocus { get; set; } = false;
    public Rectangle Bounds => _bounds;
    
    public SidePanelElement(Rectangle bounds)
    {
        _bounds = bounds;
    }
    
    public void Update(float deltaTime, IInputState inputState) { }
    
    public void Render(IUIRenderer renderer)
    {
        // Draw background
        renderer.DrawRectangle((int)_bounds.X, (int)_bounds.Y, (int)_bounds.Width, (int)_bounds.Height, UIHelper.Colors.Panel);
        renderer.DrawRectangleLines((int)_bounds.X, (int)_bounds.Y, (int)_bounds.Width, (int)_bounds.Height, UIHelper.Colors.Light);
        
        // Draw title
        renderer.DrawText("Object Info", (int)_bounds.X + 10, (int)_bounds.Y + 10, UIHelper.FontSizes.Medium, Color.White);
        
        // Draw placeholder content
        renderer.DrawText("No object selected", (int)_bounds.X + 10, (int)_bounds.Y + 50, UIHelper.FontSizes.Small, Color.Gray);
        renderer.DrawText("Click on objects in the", (int)_bounds.X + 10, (int)_bounds.Y + 70, UIHelper.FontSizes.Small, Color.Gray);
        renderer.DrawText("game view to see details", (int)_bounds.X + 10, (int)_bounds.Y + 90, UIHelper.FontSizes.Small, Color.Gray);
    }
    
    public bool HandleInput(IInputState inputState) => false;
    public bool Contains(Vector2 point) => _bounds.Contains(point);
    
    public void Resize(Rectangle newBounds)
    {
        _bounds = newBounds;
    }
}