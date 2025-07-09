using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;

namespace StarConflictsRevolt.Clients.Raylib.Renderers;

public class GameOptionsView : IView
{
    private bool _soundOn = true;
    private bool _fullscreen = false;
    public GameView ViewType => (GameView)1002;
    public void Draw()
    {
        Graphics.ClearBackground(Color.Black);
        Graphics.DrawText("Game Options", 10, 10, 28, Color.RayWhite);
        Graphics.DrawText($"Sound: {(_soundOn ? "On" : "Off")} (press S)", 10, 60, 20, Color.LightGray);
        Graphics.DrawText($"Fullscreen: {(_fullscreen ? "Yes" : "No")} (press F)", 10, 90, 20, Color.LightGray);
        if (Raylib_CSharp.Interact.Input.IsKeyPressed(Raylib_CSharp.Interact.KeyboardKey.S)) _soundOn = !_soundOn;
        if (Raylib_CSharp.Interact.Input.IsKeyPressed(Raylib_CSharp.Interact.KeyboardKey.F)) _fullscreen = !_fullscreen;
    }
} 