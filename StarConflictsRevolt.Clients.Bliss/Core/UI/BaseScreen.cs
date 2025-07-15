using Bliss.CSharp.Graphics.Rendering.Batches.Primitives;
using Bliss.CSharp.Graphics.Rendering.Batches.Sprites;
using Bliss.CSharp.Graphics.Rendering.Renderers;
using StarConflictsRevolt.Clients.Bliss.Core.UI.Interfaces;
using Veldrid;

namespace StarConflictsRevolt.Clients.Bliss.Core.UI;

/// <summary>
/// Base implementation for all UI screens.
/// Follows the Template Method pattern by providing common functionality while allowing subclasses to override specific behavior.
/// </summary>
public abstract class BaseScreen : IScreen
{
    private bool _isActive;
    private bool _isVisible = true;
    
    protected BaseScreen(string screenId, string title)
    {
        ScreenId = screenId;
        Title = title;
    }
    
    public string ScreenId { get; }
    public string Title { get; }
    
    public bool IsActive
    {
        get => _isActive;
        private set
        {
            if (_isActive != value)
            {
                _isActive = value;
                if (_isActive)
                {
                    OnActivate();
                }
                else
                {
                    OnDeactivate();
                }
            }
        }
    }
    
    public bool IsVisible
    {
        get => _isVisible;
        protected set => _isVisible = value;
    }
    
    public event Action<string>? NavigationRequested;
    public event Action? ExitRequested;
    
    public virtual void OnActivate()
    {
        // Override in derived classes to perform activation logic
    }
    
    public virtual void OnDeactivate()
    {
        // Override in derived classes to perform deactivation logic
    }
    
    public virtual void Update(float deltaTime)
    {
        // Override in derived classes to perform update logic
    }
    
    public abstract void Render(ImmediateRenderer immediateRenderer,
                               PrimitiveBatch primitiveBatch,
                               SpriteBatch spriteBatch,
                               CommandList commandList,
                               Framebuffer framebuffer);
    
    public virtual void HandleInput()
    {
        // Override in derived classes to perform input handling
    }
    
    /// <summary>
    /// Sets the active state of this screen.
    /// </summary>
    /// <param name="active">Whether the screen should be active.</param>
    internal void SetActive(bool active)
    {
        IsActive = active;
    }
    
    /// <summary>
    /// Requests navigation to another screen.
    /// </summary>
    /// <param name="screenId">The ID of the screen to navigate to.</param>
    protected void RequestNavigation(string screenId)
    {
        NavigationRequested?.Invoke(screenId);
    }
    
    /// <summary>
    /// Requests to exit the application.
    /// </summary>
    protected void RequestExit()
    {
        ExitRequested?.Invoke();
    }
} 