using System.Numerics;
using Raylib_CSharp.Colors;
using StarConflictsRevolt.Clients.Raylib.Rendering.UI;

namespace StarConflictsRevolt.Clients.Raylib.Rendering.Core;

/// <summary>
/// Bottom bar element showing status and controls.
/// </summary>
public class BottomBarElement : IUIElement
{
    private Rectangle _bounds;
    
    public string Id => "BottomBar";
    public Vector2 Position { get => new(_bounds.X, _bounds.Y); set => _bounds = new Rectangle(value.X, value.Y, _bounds.Width, _bounds.Height); }
    public Vector2 Size { get => new(_bounds.Width, _bounds.Height); set => _bounds = new Rectangle(_bounds.X, _bounds.Y, value.X, value.Y); }
    public bool IsVisible { get; set; } = true;
    public bool IsEnabled { get; set; } = true;
    public bool HasFocus { get; set; } = false;
    public Rectangle Bounds => _bounds;
    
    public BottomBarElement(Rectangle bounds)
    {
        _bounds = bounds;
    }
    
    public void Update(float deltaTime, IInputState inputState) { }
    
    public void Render(IUIRenderer renderer)
    {
        // Draw background
        renderer.DrawRectangle((int)_bounds.X, (int)_bounds.Y, (int)_bounds.Width, (int)_bounds.Height, UIHelper.Colors.Dark);
        renderer.DrawRectangleLines((int)_bounds.X, (int)_bounds.Y, (int)_bounds.Width, (int)_bounds.Height, UIHelper.Colors.Light);
        
        // Draw status text
        renderer.DrawText("Ready", (int)_bounds.X + 10, (int)_bounds.Y + 10, UIHelper.FontSizes.Small, Color.White);
        
        // Draw controls hint
        var controlsText = "ESC: Menu | Mouse: Pan | Scroll: Zoom | F1-F4: Views";
        renderer.DrawText(controlsText, (int)_bounds.X + (int)_bounds.Width - 400, (int)_bounds.Y + 10, UIHelper.FontSizes.Small, Color.Gray);
    }
    
    public bool HandleInput(IInputState inputState) => false;
    public bool Contains(Vector2 point) => _bounds.Contains(point);
    
    public void Resize(Rectangle newBounds)
    {
        _bounds = newBounds;
    }
}