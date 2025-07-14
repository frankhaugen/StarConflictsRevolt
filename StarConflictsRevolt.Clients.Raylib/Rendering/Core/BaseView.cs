using StarConflictsRevolt.Clients.Raylib.Core;

namespace StarConflictsRevolt.Clients.Raylib.Rendering.Core;

/// <summary>
/// Base class for all views providing common functionality.
/// </summary>
public abstract class BaseView : IView
{
    protected readonly RenderContext _context;
    
    protected BaseView(RenderContext context)
    {
        _context = context;
    }
    
    public abstract GameView ViewType { get; }
    
    public abstract void Draw();
    
    /// <summary>
    /// Navigate to a different view.
    /// </summary>
    /// <param name="viewType">Target view type.</param>
    protected void NavigateTo(GameView viewType)
    {
        _context.GameState.NavigateTo(viewType);
    }
} 