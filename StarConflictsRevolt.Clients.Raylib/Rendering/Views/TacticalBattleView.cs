using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Windowing;
using StarConflictsRevolt.Clients.Raylib.Core;
using StarConflictsRevolt.Clients.Raylib.Game.Commands;
using StarConflictsRevolt.Clients.Raylib.Rendering.Core;
using StarConflictsRevolt.Clients.Raylib.Rendering.UI;

namespace StarConflictsRevolt.Clients.Raylib.Rendering.Views;

public class TacticalBattleView : IView
{
    private readonly GameCommandService _commandService;
    private readonly RenderContext _renderContext;

    public TacticalBattleView(RenderContext renderContext, GameCommandService commandService)
    {
        _renderContext = renderContext;
        _commandService = commandService;
    }

    /// <inheritdoc />
    public GameView ViewType => GameView.TacticalBattle;

    /// <inheritdoc />
    public void Draw()
    {
        Graphics.ClearBackground(UIHelper.Colors.Background);

        var currentWorld = _renderContext.World;
        if (currentWorld == null)
        {
            UIHelper.DrawText("No world data available", 400, 300, UIHelper.FontSizes.Large, Color.White, true);
            return;
        }

        // Draw title
        UIHelper.DrawText("Tactical Battle View", 400, 20, UIHelper.FontSizes.Large, Color.White, true);

        // Draw battle area
        DrawBattleArea();

        // Draw battle units
        DrawBattleUnits();

        // Draw battle controls
        DrawBattleControls();

        // Draw status panel
        DrawStatusPanel();

        // Draw status bar
        UIHelper.DrawStatusBar(Window.GetScreenHeight() - 30, "Tactical Battle - Use mouse to select units, ESC: Galaxy, Backspace: Menu");

        // Handle input
        HandleInput();
    }

    private void DrawBattleArea()
    {
        var battleAreaX = 50;
        var battleAreaY = 80;
        var battleAreaWidth = 700;
        var battleAreaHeight = 400;

        // Draw battle area background
        Graphics.DrawRectangle(battleAreaX, battleAreaY, battleAreaWidth, battleAreaHeight, UIHelper.Colors.Dark);
        Graphics.DrawRectangleLines(battleAreaX, battleAreaY, battleAreaWidth, battleAreaHeight, Color.White);

        // Draw grid lines
        for (var x = 0; x <= battleAreaWidth; x += 50) Graphics.DrawLine(battleAreaX + x, battleAreaY, battleAreaX + x, battleAreaY + battleAreaHeight, Color.Gray);
        for (var y = 0; y <= battleAreaHeight; y += 50) Graphics.DrawLine(battleAreaX, battleAreaY + y, battleAreaX + battleAreaWidth, battleAreaY + y, Color.Gray);
    }

    private void DrawBattleUnits()
    {
        // Draw friendly fleet
        Graphics.DrawCircle(150, 280, 20, Color.Blue);
        Graphics.DrawText("Fleet Alpha", 120, 310, 12, Color.White);
        Graphics.DrawText("5 Ships", 120, 325, 10, Color.LightGray);

        // Draw enemy fleet
        Graphics.DrawCircle(350, 280, 20, Color.Red);
        Graphics.DrawText("Enemy Fleet", 320, 310, 12, Color.White);
        Graphics.DrawText("3 Ships", 320, 325, 10, Color.LightGray);

        // Draw neutral fleet
        Graphics.DrawCircle(550, 280, 20, Color.Green);
        Graphics.DrawText("Neutral Fleet", 520, 310, 12, Color.White);
        Graphics.DrawText("2 Ships", 520, 325, 10, Color.LightGray);

        // Draw battle lines
        Graphics.DrawLine(170, 280, 330, 280, Color.Yellow);
        Graphics.DrawLine(370, 280, 530, 280, Color.Yellow);

        // Draw weapon ranges
        Graphics.DrawCircleLines(150, 280, 40, Color.Orange);
        Graphics.DrawCircleLines(350, 280, 40, Color.Orange);
        Graphics.DrawCircleLines(550, 280, 40, Color.Orange);
    }

    private void DrawBattleControls()
    {
        var controlY = 500;
        var buttonWidth = 120;
        var buttonHeight = 30;
        var spacing = 20;
        var startX = 50;

        if (UIHelper.DrawButton("Attack", startX, controlY, buttonWidth, buttonHeight, UIHelper.Colors.Danger)) _renderContext.GameState.SetFeedback("Attack command sent", TimeSpan.FromSeconds(2));

        if (UIHelper.DrawButton("Retreat", startX + buttonWidth + spacing, controlY, buttonWidth, buttonHeight, UIHelper.Colors.Warning)) _renderContext.GameState.SetFeedback("Retreat command sent", TimeSpan.FromSeconds(2));

        if (UIHelper.DrawButton("Formation", startX + (buttonWidth + spacing) * 2, controlY, buttonWidth, buttonHeight)) _renderContext.GameState.SetFeedback("Formation dialog not implemented yet", TimeSpan.FromSeconds(3));

        if (UIHelper.DrawButton("Special", startX + (buttonWidth + spacing) * 3, controlY, buttonWidth, buttonHeight)) _renderContext.GameState.SetFeedback("Special abilities not implemented yet", TimeSpan.FromSeconds(3));

        if (UIHelper.DrawButton("Back to Galaxy", startX + (buttonWidth + spacing) * 4, controlY, buttonWidth, buttonHeight, UIHelper.Colors.Secondary)) _renderContext.GameState.NavigateTo(GameView.Galaxy);
    }

    private void DrawStatusPanel()
    {
        var info = new List<(string Label, string Value)>
        {
            ("Battle Status", "In Progress"),
            ("Turn", "3"),
            ("Friendly Ships", "5"),
            ("Enemy Ships", "3"),
            ("Neutral Ships", "2"),
            ("Terrain", "Open Space")
        };

        UIHelper.DrawInfoPanel(Window.GetScreenWidth() - 250, 80, 200, 200, "Battle Status", info);
    }

    private void HandleInput()
    {
        if (Input.IsKeyPressed(KeyboardKey.Escape)) _renderContext.GameState.NavigateTo(GameView.Galaxy);

        if (Input.IsKeyPressed(KeyboardKey.Backspace)) _renderContext.GameState.NavigateTo(GameView.Menu);

        if (Input.IsKeyPressed(KeyboardKey.Space)) _renderContext.GameState.SetFeedback("Battle paused", TimeSpan.FromSeconds(2));

        if (Input.IsKeyPressed(KeyboardKey.A)) _renderContext.GameState.SetFeedback("Auto-battle enabled", TimeSpan.FromSeconds(2));
    }
}