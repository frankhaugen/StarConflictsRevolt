using Bliss.CSharp.Graphics.Rendering.Batches.Primitives;
using Bliss.CSharp.Graphics.Rendering.Batches.Sprites;
using Bliss.CSharp.Graphics.Rendering.Renderers;
using StarConflictsRevolt.Clients.Bliss.Core.UI.Interfaces;
using Veldrid;

namespace StarConflictsRevolt.Clients.Bliss.Core.UI;

/// <summary>
/// Manages screen transitions and navigation in the application.
/// Follows the Single Responsibility Principle by focusing only on screen management.
/// </summary>
public class ScreenManager : IScreenManager
{
    private readonly Dictionary<string, IScreen> _screens = new();
    private IScreen? _currentScreen;
    private IScreen? _previousScreen;
    
    public IScreen? CurrentScreen => _currentScreen;
    public IScreen? PreviousScreen => _previousScreen;
    
    public void RegisterScreen(IScreen screen)
    {
        if (screen == null)
            throw new ArgumentNullException(nameof(screen));
            
        if (string.IsNullOrEmpty(screen.ScreenId))
            throw new ArgumentException("Screen ID cannot be null or empty.", nameof(screen));
            
        _screens[screen.ScreenId] = screen;
        
        // Set up event handlers
        screen.NavigationRequested += OnScreenNavigationRequested;
        screen.ExitRequested += OnScreenExitRequested;
        
        // Set the first registered screen as current if none is set
        if (_currentScreen == null)
        {
            NavigateTo(screen.ScreenId);
        }
    }
    
    public bool NavigateTo(string screenId)
    {
        if (string.IsNullOrEmpty(screenId))
            return false;
            
        if (!_screens.TryGetValue(screenId, out var screen))
            return false;
            
        // Deactivate current screen
        if (_currentScreen != null)
        {
            if (_currentScreen is BaseScreen currentBaseScreen)
            {
                currentBaseScreen.SetActive(false);
            }
        }
        
        // Update navigation stack
        _previousScreen = _currentScreen;
        _currentScreen = screen;
        
        // Activate new screen
        if (_currentScreen is BaseScreen newBaseScreen)
        {
            newBaseScreen.SetActive(true);
        }
        
        return true;
    }
    
    public bool NavigateBack()
    {
        if (_previousScreen == null)
            return false;
            
        return NavigateTo(_previousScreen.ScreenId);
    }
    
    public IScreen? GetScreen(string screenId)
    {
        return _screens.TryGetValue(screenId, out var screen) ? screen : null;
    }
    
    public IEnumerable<IScreen> GetAllScreens()
    {
        return _screens.Values;
    }
    
    public void Update(float deltaTime)
    {
        _currentScreen?.Update(deltaTime);
    }
    
    public void Render(ImmediateRenderer immediateRenderer,
                      PrimitiveBatch primitiveBatch,
                      SpriteBatch spriteBatch,
                      CommandList commandList,
                      Framebuffer framebuffer)
    {
        _currentScreen?.Render(immediateRenderer, primitiveBatch, spriteBatch, commandList, framebuffer);
    }
    
    public void HandleInput()
    {
        _currentScreen?.HandleInput();
    }
    
    private void OnScreenNavigationRequested(string screenId)
    {
        NavigateTo(screenId);
    }
    
    private void OnScreenExitRequested()
    {
        // This event will be handled by the application layer
        // The screen manager just propagates it
    }
} 