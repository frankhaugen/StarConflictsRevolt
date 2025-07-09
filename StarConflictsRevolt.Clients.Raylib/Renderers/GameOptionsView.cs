using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Windowing;

namespace StarConflictsRevolt.Clients.Raylib.Renderers;

public class GameOptionsView : IView
{
    private readonly RenderContext _renderContext;
    private bool _soundOn = true;
    private bool _fullscreen = false;
    private bool _showAdvancedOptions = false;
    
    public GameOptionsView(RenderContext renderContext)
    {
        _renderContext = renderContext;
    }
    
    public GameView ViewType => GameView.GameOptions;
    
    public void Draw()
    {
        Graphics.ClearBackground(UIHelper.Colors.Background);
        
        // Draw title
        UIHelper.DrawText("Game Options", 400, 30, UIHelper.FontSizes.Large, Color.White, true);
        
        var centerX = Window.GetScreenWidth() / 2;
        var startY = 100;
        var optionSpacing = 60;
        
        // Draw options
        DrawOption("Sound", _soundOn ? "On" : "Off", centerX - 150, startY, () => _soundOn = !_soundOn);
        DrawOption("Fullscreen", _fullscreen ? "Yes" : "No", centerX - 150, startY + optionSpacing, () => _fullscreen = !_fullscreen);
        DrawOption("Game Speed", _renderContext.GameState.Speed.ToString(), centerX - 150, startY + optionSpacing * 2, () => CycleGameSpeed());
        
        // Advanced options toggle
        if (UIHelper.DrawButton("Advanced Options", centerX - 150, startY + optionSpacing * 3, 300, 40, UIHelper.Colors.Secondary))
        {
            _showAdvancedOptions = !_showAdvancedOptions;
        }
        
        if (_showAdvancedOptions)
        {
            DrawAdvancedOptions(centerX - 150, startY + optionSpacing * 4 + 20);
        }
        
        // Back button
        if (UIHelper.DrawButton("Back to Menu", centerX - 150, startY + optionSpacing * 5, 300, 40, UIHelper.Colors.Primary))
        {
            _renderContext.GameState.NavigateTo(GameView.Menu);
        }
        
        // Handle keyboard shortcuts
        HandleKeyboardInput();
        
        // Draw status bar
        UIHelper.DrawStatusBar(Window.GetScreenHeight() - 30, "Press S for Sound, F for Fullscreen, G for Game Speed");
    }
    
    private void DrawOption(string label, string value, int x, int y, Action toggleAction)
    {
        UIHelper.DrawText($"{label}: {value}", x, y, UIHelper.FontSizes.Medium, Color.White);
        
        if (UIHelper.DrawButton("Toggle", x + 200, y - 5, 100, 30, UIHelper.Colors.Primary))
        {
            toggleAction();
        }
    }
    
    private void DrawAdvancedOptions(int x, int y)
    {
        UIHelper.DrawPanel(x, y, 300, 200, UIHelper.Colors.Dark);
        UIHelper.DrawText("Advanced Options", x + 10, y + 10, UIHelper.FontSizes.Medium, Color.White);
        
        UIHelper.DrawText("Graphics Quality: High", x + 10, y + 40, UIHelper.FontSizes.Small, Color.LightGray);
        UIHelper.DrawText("Network Timeout: 30s", x + 10, y + 60, UIHelper.FontSizes.Small, Color.LightGray);
        UIHelper.DrawText("Auto-Save: Enabled", x + 10, y + 80, UIHelper.FontSizes.Small, Color.LightGray);
        UIHelper.DrawText("Debug Mode: Disabled", x + 10, y + 100, UIHelper.FontSizes.Small, Color.LightGray);
        UIHelper.DrawText("Log Level: Info", x + 10, y + 120, UIHelper.FontSizes.Small, Color.LightGray);
    }
    
    private void CycleGameSpeed()
    {
        _renderContext.GameState.Speed = _renderContext.GameState.Speed switch
        {
            GameSpeed.Paused => GameSpeed.Slow,
            GameSpeed.Slow => GameSpeed.Normal,
            GameSpeed.Normal => GameSpeed.Fast,
            GameSpeed.Fast => GameSpeed.Paused,
            _ => GameSpeed.Normal
        };
    }
    
    private void HandleKeyboardInput()
    {
        if (Input.IsKeyPressed(KeyboardKey.S))
        {
            _soundOn = !_soundOn;
        }
        
        if (Input.IsKeyPressed(KeyboardKey.F))
        {
            _fullscreen = !_fullscreen;
        }
        
        if (Input.IsKeyPressed(KeyboardKey.G))
        {
            CycleGameSpeed();
        }
        
        if (Input.IsKeyPressed(KeyboardKey.Escape))
        {
            _renderContext.GameState.NavigateTo(GameView.Menu);
        }
    }
} 