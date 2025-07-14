using System.Numerics;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Windowing;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Raylib.Rendering.UI;

namespace StarConflictsRevolt.Clients.Raylib.Rendering.Core;

/// <summary>
/// Manages the in-game GUI overlay that appears on top of the game viewport.
/// </summary>
public class IngameGUI
{
    private readonly GameViewport _gameViewport;
    private readonly ILogger<IngameGUI> _logger;
    private readonly List<IUIElement> _uiElements = new();
    private readonly Dictionary<string, object> _uiState = new();
    
    // UI Layout constants
    private const int TOP_BAR_HEIGHT = 60;
    private const int SIDE_PANEL_WIDTH = 300;
    private const int BOTTOM_BAR_HEIGHT = 40;
    private const int MINIMAP_SIZE = 200;
    private const int MARGIN = 10;
    
    public IngameGUI(GameViewport gameViewport, ILogger<IngameGUI> logger)
    {
        _gameViewport = gameViewport;
        _logger = logger;
        InitializeUI();
    }
    
    private void InitializeUI()
    {
        // Top bar - Resources and player info
        var topBar = new TopBarElement(new Rectangle(0, 0, Window.GetScreenWidth(), TOP_BAR_HEIGHT));
        _uiElements.Add(topBar);
        
        // Side panel - Object info and actions
        var sidePanel = new SidePanelElement(new Rectangle(Window.GetScreenWidth() - SIDE_PANEL_WIDTH, TOP_BAR_HEIGHT, SIDE_PANEL_WIDTH, Window.GetScreenHeight() - TOP_BAR_HEIGHT - BOTTOM_BAR_HEIGHT));
        _uiElements.Add(sidePanel);
        
        // Bottom bar - Status and controls
        var bottomBar = new BottomBarElement(new Rectangle(0, Window.GetScreenHeight() - BOTTOM_BAR_HEIGHT, Window.GetScreenWidth(), BOTTOM_BAR_HEIGHT));
        _uiElements.Add(bottomBar);
        
        // Minimap
        var minimap = new MinimapElement(new Rectangle(Window.GetScreenWidth() - MINIMAP_SIZE - MARGIN, TOP_BAR_HEIGHT + MARGIN, MINIMAP_SIZE, MINIMAP_SIZE));
        _uiElements.Add(minimap);
        
        _logger.LogInformation("IngameGUI initialized with {ElementCount} UI elements", _uiElements.Count);
    }
    
    public void Update(float deltaTime, IInputState inputState, GameStateInfoDto? gameState, WorldDto? world)
    {
        // Update UI state
        _uiState["GameState"] = gameState;
        _uiState["World"] = world;
        _uiState["GameViewport"] = _gameViewport;
        
        // Update all UI elements
        foreach (var element in _uiElements)
        {
            element.Update(deltaTime, inputState);
        }
    }
    
    public void Render(IUIRenderer renderer)
    {
        // Render all UI elements
        foreach (var element in _uiElements)
        {
            if (element.IsVisible)
            {
                element.Render(renderer);
            }
        }
    }
    
    public bool HandleInput(IInputState inputState)
    {
        // Handle input in reverse order (top-most elements first)
        for (int i = _uiElements.Count - 1; i >= 0; i--)
        {
            var element = _uiElements[i];
            if (element.IsVisible && element.IsEnabled && element.HandleInput(inputState))
            {
                return true; // Input was handled
            }
        }
        return false; // No input was handled
    }
    
    public void Resize(int screenWidth, int screenHeight)
    {
        // Update UI element positions and sizes for new screen dimensions
        foreach (var element in _uiElements)
        {
            if (element is TopBarElement topBar)
            {
                topBar.Resize(new Rectangle(0, 0, screenWidth, TOP_BAR_HEIGHT));
            }
            else if (element is SidePanelElement sidePanel)
            {
                sidePanel.Resize(new Rectangle(screenWidth - SIDE_PANEL_WIDTH, TOP_BAR_HEIGHT, SIDE_PANEL_WIDTH, screenHeight - TOP_BAR_HEIGHT - BOTTOM_BAR_HEIGHT));
            }
            else if (element is BottomBarElement bottomBar)
            {
                bottomBar.Resize(new Rectangle(0, screenHeight - BOTTOM_BAR_HEIGHT, screenWidth, BOTTOM_BAR_HEIGHT));
            }
            else if (element is MinimapElement minimap)
            {
                minimap.Resize(new Rectangle(screenWidth - MINIMAP_SIZE - MARGIN, TOP_BAR_HEIGHT + MARGIN, MINIMAP_SIZE, MINIMAP_SIZE));
            }
        }
        
        _logger.LogDebug("IngameGUI resized to {Width}x{Height}", screenWidth, screenHeight);
    }
}

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