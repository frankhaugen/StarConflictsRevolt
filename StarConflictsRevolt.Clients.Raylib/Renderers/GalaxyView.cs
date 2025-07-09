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
        if (currentWorld == null)
            return;

        // Draw a galaxy backdrop with systems oversized:
        var systems = currentWorld.Galaxy.StarSystems;
        if (systems == null || !systems.Any())
        {
            Graphics.DrawText("No systems found in the galaxy.", 10, 10, 20, Color.RayWhite);
            return;
        }

        // Draw star systems and planets
        foreach (var system in systems)
        {
            // Draw system
            Graphics.DrawCircle((int)system.Coordinates.X, (int)system.Coordinates.Y, 16, Color.Yellow);
            Graphics.DrawText(system.Name, (int)(system.Coordinates.X + 18), (int)(system.Coordinates.Y - 8), 12, Color.White);

            // Draw planets around the system
            if (system.Planets != null)
            {
                double angleStep = 2 * Math.PI / Math.Max(1, system.Planets.Count());
                int radius = 32;
                int i = 0;
                foreach (var planet in system.Planets)
                {
                    var px = (int)(system.Coordinates.X + Math.Cos(i * angleStep) * radius);
                    var py = (int)(system.Coordinates.Y + Math.Sin(i * angleStep) * radius);
                    Graphics.DrawCircle(px, py, 6, Color.Blue);
                    Graphics.DrawText(planet.Name, px + 8, py - 8, 10, Color.LightBlue);
                    i++;
                }
            }
        }

        // Selection logic (mouse click)
        if (Input.IsMouseButtonPressed(MouseButton.Left))
        {
            var mouse = Input.GetMousePosition();
            foreach (var system in systems)
            {
                var dist = Math.Sqrt(Math.Pow(mouse.X - system.Coordinates.X, 2) + Math.Pow(mouse.Y - system.Coordinates.Y, 2));
                if (dist < 16)
                {
                    _renderContext.SelectedObject = system;
                    break;
                }
                if (system.Planets != null)
                {
                    double angleStep = 2 * Math.PI / Math.Max(1, system.Planets.Count());
                    int radius = 32;
                    int i = 0;
                    foreach (var planet in system.Planets)
                    {
                        var px = (int)(system.Coordinates.X + Math.Cos(i * angleStep) * radius);
                        var py = (int)(system.Coordinates.Y + Math.Sin(i * angleStep) * radius);
                        var pdist = Math.Sqrt(Math.Pow(mouse.X - px, 2) + Math.Pow(mouse.Y - py, 2));
                        if (pdist < 6)
                        {
                            _renderContext.SelectedObject = planet;
                            break;
                        }
                        i++;
                    }
                }
            }
        }

        // Info panel for selected object
        var selected = _renderContext.SelectedObject;
        string feedback = string.Empty;
        if (selected != null)
        {
            string info = selected switch
            {
                var s when s.GetType().Name.Contains("StarSystemDto") => $"System: {((dynamic)s).Name}",
                var p when p.GetType().Name.Contains("PlanetDto") => $"Planet: {((dynamic)p).Name}",
                _ => $"Selected: {selected.GetType().Name}"
            };
            Graphics.DrawRectangle(0, Window.GetScreenHeight() - 80, Window.GetScreenWidth(), 80, Color.DarkGray);
            Graphics.DrawText(info, 10, Window.GetScreenHeight() - 70, 20, Color.White);
            Graphics.DrawText("[M]ove  [B]uild  [A]ttack  [D]iplomacy", 10, Window.GetScreenHeight() - 40, 18, Color.LightGray);
            if (!string.IsNullOrEmpty(feedback))
                Graphics.DrawText(feedback, 10, Window.GetScreenHeight() - 20, 16, Color.Green);

            // Action keys
            if (Input.IsKeyPressed(KeyboardKey.M))
            {
                // MoveFleetEvent example (stub: use real IDs)
                _ = SendActionAsync("/game/move-fleet", new { PlayerId = Guid.NewGuid(), FleetId = Guid.NewGuid(), FromPlanetId = Guid.NewGuid(), ToPlanetId = Guid.NewGuid() });
                feedback = "Move command sent.";
            }
            if (Input.IsKeyPressed(KeyboardKey.B))
            {
                _ = SendActionAsync("/game/build-structure", new { PlayerId = Guid.NewGuid(), PlanetId = Guid.NewGuid(), StructureType = "Mine" });
                feedback = "Build command sent.";
            }
            if (Input.IsKeyPressed(KeyboardKey.A))
            {
                _ = SendActionAsync("/game/attack", new { PlayerId = Guid.NewGuid(), AttackerFleetId = Guid.NewGuid(), DefenderFleetId = Guid.NewGuid(), LocationPlanetId = Guid.NewGuid() });
                feedback = "Attack command sent.";
            }
            if (Input.IsKeyPressed(KeyboardKey.D))
            {
                _ = SendActionAsync("/game/diplomacy", new { PlayerId = Guid.NewGuid(), TargetPlayerId = Guid.NewGuid(), ProposalType = "Alliance", Message = "Let's be friends!" });
                feedback = "Diplomacy command sent.";
            }
        }

        // Keyboard shortcuts for view switching (F1-F7)
        if (Input.IsKeyPressed(KeyboardKey.F1)) _renderContext.CurrentView = GameView.Menu;
        // Add more as needed for other views
        if (Input.IsKeyPressed(KeyboardKey.Escape) || Input.IsKeyPressed(KeyboardKey.Q))
        {
            _renderContext.CurrentView = GameView.Menu;
        }
        Graphics.DrawText($"Current View: {ViewType}", 10, 10, 20, Color.RayWhite);
    }

    private async Task SendActionAsync(string endpoint, object payload)
    {
        try
        {
            using var httpClient = new System.Net.Http.HttpClient();
            if (!string.IsNullOrEmpty(_renderContext.AccessToken))
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _renderContext.AccessToken);
            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"http://localhost:5267{endpoint}", content);
            // Optionally handle response
        }
        catch (Exception ex)
        {
            // Optionally log error
        }
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