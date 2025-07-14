using System.Numerics;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Windowing;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Raylib.Core;
using StarConflictsRevolt.Clients.Raylib.Rendering.Core;
using StarConflictsRevolt.Clients.Raylib.Rendering.UI;

namespace StarConflictsRevolt.Clients.Raylib.Rendering.Views;

public class GalaxyView(RenderContext renderContext, ILogger<GalaxyView> logger) : IView
{
    /// <inheritdoc />
    public GameView ViewType => GameView.Galaxy;

    private int _starfieldFrame = 0;
    private StarSystemWithSector? _hoveredSystem = null;
    private StarSystemWithSector? _selectedSystem = null;
    private bool _showSystemDialog = false;
    private StarSystemWithSector? _dialogSystem = null;
    private SectorInfo? _selectedSector = null;
    private bool _showSectorDialog = false;

    private static readonly int CanvasX = 64;
    private static readonly int CanvasY = 48;
    private static readonly int CanvasW = 1536;
    private static readonly int CanvasH = 864;

    public void Draw()
    {
        Graphics.ClearBackground(UIHelper.Colors.Background);
        _starfieldFrame++;
        // Main galaxy canvas (pans/zooms)
        Raylib_CSharp.Rendering.Graphics.BeginScissorMode(CanvasX, CanvasY, CanvasW, CanvasH);
        Raylib_CSharp.Rendering.Graphics.BeginMode2D(renderContext.UIManager.Camera);
        DrawSectorBorders();
        DrawGalaxy();
        if (IsMouseInGalaxyCanvas())
        {
            HandleMouseHover();
            HandleMouseSelection();
        }
        Raylib_CSharp.Rendering.Graphics.EndMode2D();
        Raylib_CSharp.Rendering.Graphics.EndScissorMode();
        DrawStarfield(); // screen space, after camera
        DrawLeftToolbar(); // screen space, after camera
        DrawTopStatusBar(); // screen space, after camera
        DrawRightSidebar(); // screen space, after camera
        DrawBottomConsole(); // screen space, after camera
        UIHelper.DrawReticle(); // screen space
        if (_showSystemDialog && _dialogSystem != null)
        {
            ShowSystemDialog(_dialogSystem);
        }
        else if (_showSectorDialog && _selectedSector != null)
        {
            ShowSectorDialog(_selectedSector);
        }
    }

    private bool IsMouseInGalaxyCanvas()
    {
        var mouse = Input.GetMousePosition();
        return mouse.X >= 64 && mouse.X < 1600 && mouse.Y >= 48 && mouse.Y < 912;
    }

    private void DrawStarfield()
    {
        // Draw in screen space (not affected by camera)
        int numStars = 2000;
        double spiralArms = 3.5;
        double spiralSpread = 0.45;
        double centerX = Window.GetScreenWidth() / 2.0;
        double centerY = Window.GetScreenHeight() / 2.0;
        double maxRadius = Math.Min(centerX, centerY) * 1.1;
        var rand = new Random(42);
        for (int i = 0; i < numStars; i++)
        {
            // Spiral math
            double t = i * 0.15;
            double arm = (i % (int)spiralArms) * (2 * Math.PI / spiralArms);
            double radius = spiralSpread * t * maxRadius / numStars + rand.NextDouble() * 18;
            double angle = t + arm + rand.NextDouble() * 0.25;
            int x = (int)(centerX + Math.Cos(angle) * radius + rand.NextDouble() * 8 - 4);
            int y = (int)(centerY + Math.Sin(angle) * radius + rand.NextDouble() * 8 - 4);
            // Twinkle
            byte twinkle = (byte)(((_starfieldFrame / 10 + i) % 20 < 2) ? 40 : 0);
            byte baseCol = (byte)(120 + rand.Next(80));
            var color = (i % 7 == 0)
                ? new Color((byte)(200 + twinkle), (byte)(200 + twinkle), 255, 255)
                : new Color((byte)(baseCol + twinkle), (byte)(baseCol + twinkle), (byte)(180 + rand.Next(60)), 255);
            Graphics.DrawPixel(x, y, color);
        }
    }

    private void DrawSectorBorders()
    {
        foreach (var sector in GalaxyLayout.GetSectors())
        {
            int sx = 100 + sector.SectorId * 300;
            UIHelper.DrawSciFiBorder(sx, 0, 300, CanvasH, new Color(30, 30, 30, 180), 2);
        }
    }

    private void DrawGalaxy()
    {
        foreach (var sector in GalaxyLayout.GetSectors())
        {
            if (!sector.IsVisible)
            {
                int sx = 100 + sector.SectorId * 300;
                Graphics.DrawRectangle(sx, 0, 300, CanvasH, new Color(30, 30, 30, 180));
                UIHelper.DrawText($"{sector.Name} (Hidden)", sx + 20, 60, UIHelper.FontSizes.Medium, Color.Gray);
            }
        }
        var systems = GalaxyLayout.GetAllSystems();
        foreach (var sys in systems)
        {
            if (!sys.IsVisible) continue;
            var pos = sys.System.Coordinates;
            // Glow
            for (int r = 18; r > 8; r -= 2)
                Graphics.DrawCircle((int)pos.X, (int)pos.Y, r, new Color(255, 255, 100, 30));
            // Main system dot
            Graphics.DrawCircle((int)pos.X, (int)pos.Y, 8, Color.Yellow);
            // Demo: draw a small planet icon
            Graphics.DrawCircle((int)pos.X + 12, (int)pos.Y + 6, 3, Color.Blue);
            // Demo: draw a small fleet icon
            Graphics.DrawRectangle((int)pos.X - 10, (int)pos.Y + 10, 6, 3, Color.LightGray);
            // Name
            UIHelper.DrawText(sys.System.Name, (int)pos.X + 16, (int)pos.Y - 8, 14, Color.White);
        }
        // Hover effect
        if (_hoveredSystem != null)
        {
            var pos = _hoveredSystem.System.Coordinates;
            Graphics.DrawCircle((int)pos.X, (int)pos.Y, 16, Color.SkyBlue);
        }
        // Selection effect
        if (_selectedSystem != null)
        {
            var pos = _selectedSystem.System.Coordinates;
            Graphics.DrawCircle((int)pos.X, (int)pos.Y, 24, Color.SkyBlue);
        }
    }

    private void HandleMouseHover()
    {
        var mouse = Input.GetMousePosition();
        _hoveredSystem = null;
        var systems = GalaxyLayout.GetAllSystems();
        foreach (var sys in systems)
        {
            if (!sys.IsVisible) continue;
            var pos = sys.System.Coordinates;
            var dist = Math.Sqrt(Math.Pow(mouse.X - pos.X, 2) + Math.Pow(mouse.Y - pos.Y, 2));
            if (dist < 16)
            {
                _hoveredSystem = sys;
                break;
            }
        }
    }

    private void HandleMouseSelection()
    {
        if (!Input.IsMouseButtonPressed(MouseButton.Left)) return;
        var mouse = Input.GetMousePosition();
        var systems = GalaxyLayout.GetAllSystems();
        // Priority: system click
        foreach (var sys in systems)
        {
            if (!sys.IsVisible) continue;
            var pos = sys.System.Coordinates;
            var dist = Math.Sqrt(Math.Pow(mouse.X - pos.X, 2) + Math.Pow(mouse.Y - pos.Y, 2));
            if (dist < 16)
            {
                _selectedSystem = sys;
                _showSystemDialog = true;
                _showSectorDialog = false;
                _dialogSystem = sys;
                _selectedSector = null;
                renderContext.GameState.SelectedObject = sys.System;
                renderContext.GameState.SetFeedback($"Selected system: {sys.System.Name}", TimeSpan.FromSeconds(2));
                return;
            }
        }
        // Only check sector click if no system was clicked
        foreach (var sector in GalaxyLayout.GetSectors())
        {
            int sx = 100 + sector.SectorId * 300;
            int ex = sx + 300;
            if (mouse.X >= sx && mouse.X < ex && mouse.Y >= 0 && mouse.Y < CanvasH)
            {
                _selectedSector = sector;
                _showSectorDialog = true;
                _showSystemDialog = false;
                _selectedSystem = null;
                _dialogSystem = null;
                renderContext.GameState.SetFeedback($"Selected sector: {sector.Name}", TimeSpan.FromSeconds(2));
                return;
            }
        }
    }

    private void DrawActionPanel()
    {
        if (_selectedSystem == null) return;
        var system = _selectedSystem.System;
        var info = new List<(string Label, string Value)>
        {
            ("System", system.Name),
            ("Coordinates", $"({system.Coordinates.X:F0}, {system.Coordinates.Y:F0})"),
            ("Planets", $"{system.Planets?.Count() ?? 0}")
        };
        UIHelper.DrawInfoPanel(10, Window.GetScreenHeight() - 200, 300, 150, "Selected System", info);
    }

    private void HandleKeyboardInput()
    {
        if (Input.IsKeyPressed(KeyboardKey.Escape) || Input.IsKeyPressed(KeyboardKey.Backspace))
        {
            renderContext.GameState.NavigateTo(GameView.Menu);
        }
        if (Input.IsKeyPressed(KeyboardKey.F5)) GalaxyLayout.ToggleSectorVisibility(1);
        if (Input.IsKeyPressed(KeyboardKey.F6)) GalaxyLayout.ToggleSectorVisibility(2);
        if (Input.IsKeyPressed(KeyboardKey.F7)) GalaxyLayout.ToggleSectorVisibility(3);
        if (Input.IsKeyPressed(KeyboardKey.F8)) GalaxyLayout.ToggleSectorVisibility(4);
    }

    private void ShowMoveFleetDialog()
    {
        renderContext.GameState.SetFeedback("Move fleet dialog not implemented yet", TimeSpan.FromSeconds(3));
    }

    private void ShowBuildStructureDialog()
    {
        renderContext.GameState.SetFeedback("Build structure dialog not implemented yet", TimeSpan.FromSeconds(3));
    }

    private void ShowAttackDialog()
    {
        renderContext.GameState.SetFeedback("Attack dialog not implemented yet", TimeSpan.FromSeconds(3));
    }

    private void ShowDiplomacyDialog()
    {
        renderContext.GameState.SetFeedback("Diplomacy dialog not implemented yet", TimeSpan.FromSeconds(3));
    }

    private void ShowSystemDialog(StarSystemWithSector sys)
    {
        // Placeholder: draw a sci-fi panel in the center with system info
        int w = 400, h = 250;
        int x = (Window.GetScreenWidth() - w) / 2;
        int y = (Window.GetScreenHeight() - h) / 2;
        UIHelper.DrawSciFiBorder(x, y, w, h, new Color(50, 50, 50, 200), 2);
        UIHelper.DrawText($"System: {sys.System.Name}", x + 30, y + 40, UIHelper.FontSizes.Large, Color.White);
        UIHelper.DrawText($"Sector: {sys.SectorName}", x + 30, y + 80, UIHelper.FontSizes.Medium, Color.LightGray);
        UIHelper.DrawText($"Planets: {sys.System.Planets?.Count() ?? 0}", x + 30, y + 120, UIHelper.FontSizes.Medium, Color.LightGray);
        if (UIHelper.DrawButton("Close", x + w - 110, y + h - 50, 100, 40))
        {
            _showSystemDialog = false;
            _dialogSystem = null;
        }
    }

    private void ShowSectorDialog(SectorInfo sector)
    {
        int w = 400, h = 200;
        int x = (Window.GetScreenWidth() - w) / 2;
        int y = (Window.GetScreenHeight() - h) / 2;
        UIHelper.DrawSciFiBorder(x, y, w, h, new Color(50, 50, 50, 200), 2);
        UIHelper.DrawText($"Sector: {sector.Name}", x + 30, y + 40, UIHelper.FontSizes.Large, Color.White);
        UIHelper.DrawText($"Visible: {(sector.IsVisible ? "Yes" : "No")}", x + 30, y + 80, UIHelper.FontSizes.Medium, Color.LightGray);
        if (UIHelper.DrawButton(sector.IsVisible ? "Hide" : "Reveal", x + 30, y + h - 50, 100, 40))
        {
            GalaxyLayout.ToggleSectorVisibility(sector.SectorId);
            _showSectorDialog = false;
            _selectedSector = null;
        }
        if (UIHelper.DrawButton("Close", x + w - 110, y + h - 50, 100, 40))
        {
            _showSectorDialog = false;
            _selectedSector = null;
        }
    }

    private void DrawLeftToolbar()
    {
        // Toolbar background
        Graphics.DrawRectangle(0, 0, 64, Window.GetScreenHeight(), new Color(17, 23, 34, 255));
        // Buttons
        int btnW = 40, btnH = 40, btnX = 12, btnY = 24, btnSpacing = 64;
        for (int i = 0; i < 4; i++)
        {
            int y = btnY + i * btnSpacing;
            Graphics.DrawRectangle(btnX, y, btnW, btnH, new Color(0, 191, 255, 255));
            UIHelper.DrawText($"Btn{i+1}", btnX + 8, y + 12, 16, Color.Black);
        }
    }

    private void DrawTopStatusBar()
    {
        Graphics.DrawRectangle(64, 0, Window.GetScreenWidth() - 64, 48, new Color(17, 23, 34, 255));
        UIHelper.DrawText("Game Name", 80, 32, 20, new Color(0, 191, 255, 255));
        UIHelper.DrawText("Day 42  |  17:23  |  Credits 123 456", 450, 32, 16, new Color(154, 205, 254, 255));
    }

    private void DrawRightSidebar()
    {
        int sidebarW = 320, sidebarH = Window.GetScreenHeight() - 48, sidebarX = Window.GetScreenWidth() - sidebarW, sidebarY = 48;
        Graphics.DrawRectangle(sidebarX, sidebarY, sidebarW, sidebarH, new Color(17, 23, 34, 255));
        UIHelper.DrawText("Details", sidebarX + 12, sidebarY + 36, 18, new Color(0, 191, 255, 255));
        // Resource bars (placeholders)
        int barY = sidebarY + 60;
        for (int i = 0; i < 4; i++)
        {
            Graphics.DrawRectangle(sidebarX + 12, barY + i * 40, 296, 8, new Color(32, 50, 77, 255));
            Graphics.DrawRectangle(sidebarX + 12, barY + i * 40, 220, 8, new Color(0, 191, 255, 255));
        }
    }

    private void DrawBottomConsole()
    {
        int consoleH = 168, consoleY = Window.GetScreenHeight() - consoleH;
        Graphics.DrawRectangle(64, consoleY, Window.GetScreenWidth() - 64, consoleH, new Color(17, 23, 34, 255));
        UIHelper.DrawText("Event Log", 76, consoleY + 36, 18, new Color(0, 191, 255, 255));
        UIHelper.DrawText("[17:23] Scanned system Delta-7 …", 76, consoleY + 66, 14, new Color(154, 205, 254, 255));
    }
}