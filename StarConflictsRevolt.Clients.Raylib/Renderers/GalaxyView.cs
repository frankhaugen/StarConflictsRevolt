using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Windowing;

namespace StarConflictsRevolt.Clients.Raylib.Renderers;

public class GalaxyView : IView
{
    private readonly RenderContext _renderContext;

    public GalaxyView(RenderContext renderContext)
    {
        _renderContext = renderContext;
    }
    
    /// <inheritdoc />
    public GameView ViewType => GameView.Galaxy;
    
    /// <inheritdoc />
    public void Draw()
    {
        var currentWorld = _renderContext.World;
        
        if (currentWorld != null)
            return;
        
        // Window and BeginDraw set "outside"
        
        // Draw a galaxy backdrop with systems oversized:
        var systems = currentWorld?.Galaxy.StarSystems;
        if (systems == null || !systems.Any())
        {
            // Draw a placeholder or empty state if no systems are available
            Graphics.DrawText("No systems found in the galaxy.", 10, 10, 20, Color.RayWhite);
            return;
        }

        foreach (var system in systems)
        {
            // Draw each system oversized
            Graphics.DrawCircle((int)system.Coordinates.X, (int)system.Coordinates.Y, system.Planets.Count() * 2, Color.Yellow);
            Graphics.DrawText(system.Name, (int)(system.Coordinates.X + 5), (int)(system.Coordinates.Y + 5), 10, Color.White);
        }
        
        // Handle input for galaxy navigation
        if (Input.IsKeyPressed(KeyboardKey.Escape) || Input.IsKeyPressed(KeyboardKey.Q))
        {
            _renderContext.CurrentView = GameView.Menu; // Switch to Menu view
        }
        
        // Draw the current view type for debugging
        Graphics.DrawText($"Current View: {ViewType}", 10, 10, 20, Color.RayWhite);
    }
}

public class MenuView : IView
{
    private readonly RenderContext _renderContext;

    public MenuView(RenderContext renderContext)
    {
        _renderContext = renderContext;
    }
    
    /// <inheritdoc />
    public GameView ViewType => GameView.Menu;
    
    /// <inheritdoc />
    public void Draw()
    {
        // Draw the menu background
        Graphics.ClearBackground(Color.Black);
        
        // Draw menu title
        Graphics.DrawText("Galaxy Menu", 10, 10, 20, Color.RayWhite);
        
        // Draw options
        Graphics.DrawText("1. View Galaxy", 10, 50, 20, Color.RayWhite);
        Graphics.DrawText("2. Exit", 10, 80, 20, Color.RayWhite);
        
        // Handle input for menu selection (not implemented here)
        if (Input.IsKeyPressed(KeyboardKey.One))
        {
            _renderContext.CurrentView = GameView.Galaxy; // Switch to Galaxy view
        }
        if (Input.IsKeyPressed(KeyboardKey.Two))
        {
            Window.Close(); // Exit the application
        }
        else if (Input.IsKeyPressed(KeyboardKey.Escape) || Input.IsKeyPressed(KeyboardKey.Q))
        {
            Window.Close(); // Exit the application
        }
    }
}