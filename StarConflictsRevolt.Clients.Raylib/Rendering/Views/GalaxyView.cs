using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Windowing;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Raylib.Core;
using StarConflictsRevolt.Clients.Raylib.Game.Commands;
using StarConflictsRevolt.Clients.Raylib.Rendering.Core;
using StarConflictsRevolt.Clients.Raylib.Rendering.UI;

namespace StarConflictsRevolt.Clients.Raylib.Rendering.Views;

public class GalaxyView : IView
{
    private readonly GameCommandService _commandService;
    private readonly ILogger<GalaxyView> _logger;
    private readonly RenderContext _renderContext;

    public GalaxyView(RenderContext renderContext, GameCommandService commandService, ILogger<GalaxyView> logger)
    {
        _renderContext = renderContext;
        _commandService = commandService;
        _logger = logger;
    }

    /// <inheritdoc />
    public GameView ViewType => GameView.Galaxy;

    /// <inheritdoc />
    public void Draw()
    {
        Graphics.ClearBackground(UIHelper.Colors.Background);

        // Draw resource bar (HUD)
        UIHelper.DrawResourceBar(0, 0, Window.GetScreenWidth(), 48, _renderContext.GameState.PlayerState);

        var currentWorld = _renderContext.World;
        _logger.LogDebug("GalaxyView Draw - Current world: {WorldId}, Has Galaxy: {HasGalaxy}, StarSystems: {StarSystemCount}",
            currentWorld?.Id, currentWorld?.Galaxy != null, currentWorld?.Galaxy?.StarSystems?.Count() ?? 0);

        if (currentWorld == null)
        {
            UIHelper.DrawText("No world data available", 400, 300, UIHelper.FontSizes.Large, Color.White, true);
            UIHelper.DrawText("Press Backspace or Escape to return to menu", 400, 350, UIHelper.FontSizes.Medium, Color.Gray, true);
            return;
        }

        // Draw title
        UIHelper.DrawText("Galaxy View", 400, 20, UIHelper.FontSizes.Large, Color.White, true);

        // Draw star systems and planets
        DrawGalaxy(currentWorld);

        // Handle mouse selection
        HandleMouseSelection(currentWorld);

        // Draw action panel
        DrawActionPanel();

        // Draw minimap
        UIHelper.DrawMinimap(Window.GetScreenWidth() - 200, 50, 180, 120, currentWorld);

        // Draw status bar
        UIHelper.DrawStatusBar(Window.GetScreenHeight() - 30, $"Systems: {currentWorld.Galaxy?.StarSystems?.Count() ?? 0} | Selected: {_renderContext.GameState.SelectedObject?.GetType().Name ?? "None"} | ESC/Backspace: Menu");

        // Handle keyboard input
        HandleKeyboardInput();
    }

    private void DrawGalaxy(WorldDto world)
    {
        var systems = world.Galaxy?.StarSystems;
        if (systems == null || !systems.Any())
        {
            UIHelper.DrawText("No systems found in the galaxy.", 400, 300, UIHelper.FontSizes.Medium, Color.White, true);
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
                var angleStep = 2 * Math.PI / Math.Max(1, system.Planets.Count());
                var radius = 32;
                var i = 0;
                foreach (var planet in system.Planets)
                {
                    var px = (int)(system.Coordinates.X + Math.Cos(i * angleStep) * radius);
                    var py = (int)(system.Coordinates.Y + Math.Sin(i * angleStep) * radius);

                    // Highlight selected planet
                    var planetColor = _renderContext.GameState.SelectedObject?.Id == planet.Id ? Color.Red : Color.Blue;
                    Graphics.DrawCircle(px, py, 6, planetColor);
                    Graphics.DrawText(planet.Name, px + 8, py - 8, 10, Color.SkyBlue);
                    i++;
                }
            }
        }
    }

    private void HandleMouseSelection(WorldDto world)
    {
        if (!Input.IsMouseButtonPressed(MouseButton.Left)) return;

        var mouse = Input.GetMousePosition();
        var systems = world.Galaxy?.StarSystems;
        if (systems == null) return;

        foreach (var system in systems)
        {
            var dist = Math.Sqrt(Math.Pow(mouse.X - system.Coordinates.X, 2) + Math.Pow(mouse.Y - system.Coordinates.Y, 2));
            if (dist < 16)
            {
                _renderContext.GameState.SelectedObject = system;
                _renderContext.GameState.SetFeedback($"Selected system: {system.Name}", TimeSpan.FromSeconds(2));
                return;
            }

            if (system.Planets != null)
            {
                var angleStep = 2 * Math.PI / Math.Max(1, system.Planets.Count());
                var radius = 32;
                var i = 0;
                foreach (var planet in system.Planets)
                {
                    var px = (int)(system.Coordinates.X + Math.Cos(i * angleStep) * radius);
                    var py = (int)(system.Coordinates.Y + Math.Sin(i * angleStep) * radius);
                    var pdist = Math.Sqrt(Math.Pow(mouse.X - px, 2) + Math.Pow(mouse.Y - py, 2));
                    if (pdist < 6)
                    {
                        _renderContext.GameState.SelectedObject = planet;
                        _renderContext.GameState.SetFeedback($"Selected planet: {planet.Name}", TimeSpan.FromSeconds(2));
                        return;
                    }

                    i++;
                }
            }
        }
    }

    private void DrawActionPanel()
    {
        var selected = _renderContext.GameState.SelectedObject;
        if (selected == null) return;

        // Draw info panel
        var info = new List<(string Label, string Value)>();

        if (selected is PlanetDto planet)
        {
            info.Add(("Planet", planet.Name));
            info.Add(("Radius", $"{planet.Radius:F1}"));
            info.Add(("Mass", $"{planet.Mass:F1}"));
            info.Add(("Distance", $"{planet.DistanceFromSun:F1}"));
        }
        else if (selected is StarSystemDto system)
        {
            info.Add(("System", system.Name));
            info.Add(("Planets", $"{system.Planets?.Count() ?? 0}"));
            info.Add(("Coordinates", $"({system.Coordinates.X:F0}, {system.Coordinates.Y:F0})"));
        }

        UIHelper.DrawInfoPanel(10, Window.GetScreenHeight() - 200, 300, 150, "Selected Object", info);

        // Draw action buttons
        var buttonY = Window.GetScreenHeight() - 80;
        var buttonWidth = 100;
        var buttonHeight = 30;
        var spacing = 20;
        var startX = 320;

        if (UIHelper.DrawButton("Move Fleet", startX, buttonY, buttonWidth, buttonHeight)) ShowMoveFleetDialog();

        if (UIHelper.DrawButton("Build Structure", startX + buttonWidth + spacing, buttonY, buttonWidth, buttonHeight)) ShowBuildStructureDialog();

        if (UIHelper.DrawButton("Attack", startX + (buttonWidth + spacing) * 2, buttonY, buttonWidth, buttonHeight)) ShowAttackDialog();

        if (UIHelper.DrawButton("Diplomacy", startX + (buttonWidth + spacing) * 3, buttonY, buttonWidth, buttonHeight)) ShowDiplomacyDialog();
    }

    private void HandleKeyboardInput()
    {
        if (Input.IsKeyPressed(KeyboardKey.Escape))
        {
            _logger.LogDebug("Escape key pressed - navigating to menu");
            _renderContext.GameState.NavigateTo(GameView.Menu);
        }

        if (Input.IsKeyPressed(KeyboardKey.Backspace))
        {
            _logger.LogDebug("Backspace key pressed - navigating to menu");
            _renderContext.GameState.NavigateTo(GameView.Menu);
        }

        if (Input.IsKeyPressed(KeyboardKey.F1)) _renderContext.GameState.NavigateTo(GameView.Menu);

        if (Input.IsKeyPressed(KeyboardKey.F2)) _renderContext.GameState.NavigateTo(GameView.FleetFinder);

        if (Input.IsKeyPressed(KeyboardKey.F3)) _renderContext.GameState.NavigateTo(GameView.GameOptions);

        if (Input.IsKeyPressed(KeyboardKey.F4)) _renderContext.GameState.NavigateTo(GameView.PlanetaryFinder);
    }

    private void ShowMoveFleetDialog()
    {
        _renderContext.GameState.SetFeedback("Move fleet dialog not implemented yet", TimeSpan.FromSeconds(3));
    }

    private void ShowBuildStructureDialog()
    {
        _renderContext.GameState.SetFeedback("Build structure dialog not implemented yet", TimeSpan.FromSeconds(3));
    }

    private void ShowAttackDialog()
    {
        _renderContext.GameState.SetFeedback("Attack dialog not implemented yet", TimeSpan.FromSeconds(3));
    }

    private void ShowDiplomacyDialog()
    {
        _renderContext.GameState.SetFeedback("Diplomacy dialog not implemented yet", TimeSpan.FromSeconds(3));
    }
}