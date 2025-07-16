using Raylib_CSharp.Windowing;
using StarConflictsRevolt.Clients.Models;

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