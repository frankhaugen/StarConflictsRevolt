using System.Numerics;
using Raylib_CSharp.Colors;
using StarConflictsRevolt.Clients.Raylib.Rendering.UI;

namespace StarConflictsRevolt.Clients.Raylib.Rendering.Core;

/// <summary>
/// Top bar element showing resources and player information.
/// </summary>
public class TopBarElement : IUIElement
{
    private Rectangle _bounds;
    
    public string Id => "TopBar";
    public Vector2 Position { get => new(_bounds.X, _bounds.Y); set => _bounds = new Rectangle(value.X, value.Y, _bounds.Width, _bounds.Height); }
    public Vector2 Size { get => new(_bounds.Width, _bounds.Height); set => _bounds = new Rectangle(_bounds.X, _bounds.Y, value.X, value.Y); }
    public bool IsVisible { get; set; } = true;
    public bool IsEnabled { get; set; } = true;
    public bool HasFocus { get; set; } = false;
    public Rectangle Bounds => _bounds;
    
    public TopBarElement(Rectangle bounds)
    {
        _bounds = bounds;
    }
    
    public void Update(float deltaTime, IInputState inputState) { }
    
    public void Render(IUIRenderer renderer)
    {
        // Draw background
        renderer.DrawRectangle((int)_bounds.X, (int)_bounds.Y, (int)_bounds.Width, (int)_bounds.Height, UIHelper.Colors.Dark);
        renderer.DrawRectangleLines((int)_bounds.X, (int)_bounds.Y, (int)_bounds.Width, (int)_bounds.Height, UIHelper.Colors.Light);
        
        // Draw title
        renderer.DrawText("Star Conflicts Revolt", (int)_bounds.X + 10, (int)_bounds.Y + 10, UIHelper.FontSizes.Large, Color.White);
        
        // Draw resource bars (placeholder)
        var resourceY = (int)_bounds.Y + 35;
        renderer.DrawText("Credits: 1,000,000", (int)_bounds.X + 200, resourceY, UIHelper.FontSizes.Small, Color.White);
        renderer.DrawText("Minerals: 50,000", (int)_bounds.X + 350, resourceY, UIHelper.FontSizes.Small, Color.White);
        renderer.DrawText("Energy: 25,000", (int)_bounds.X + 500, resourceY, UIHelper.FontSizes.Small, Color.White);
    }
    
    public bool HandleInput(IInputState inputState) => false;
    public bool Contains(Vector2 point) => _bounds.Contains(point);
    
    public void Resize(Rectangle newBounds)
    {
        _bounds = newBounds;
    }
}