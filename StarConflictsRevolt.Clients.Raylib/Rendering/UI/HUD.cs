using System.Numerics;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;
using StarConflictsRevolt.Clients.Raylib.Rendering.Core;

namespace StarConflictsRevolt.Clients.Raylib.Rendering.UI;

public class HUD : IUIElement
{
    public string Id { get; set; } = "HUD";
    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }
    public Rectangle Bounds { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool IsEnabled { get; set; } = true;
    public bool HasFocus { get; set; }

    public int Credits { get; set; }
    public int Materials { get; set; }
    public int Fuel { get; set; }
    public string PlayerName { get; set; } = "Player";
    public int Turn { get; set; }
    public float GameSpeed { get; set; } = 1.0f;

    public NotificationManager Notifications { get; } = new();

    public void Update(float deltaTime, IInputState inputState)
    {
        Notifications.Update(deltaTime, inputState);
    }

    public void Render(IUIRenderer renderer)
    {
        DrawTopBar();
        Notifications.Render(renderer);
    }

    private static Raylib_CSharp.Transformations.Rectangle ToRaylibRect(Rectangle r)
    {
        return new Raylib_CSharp.Transformations.Rectangle(r.X, r.Y, r.Width, r.Height);
    }

    private void DrawTopBar()
    {
        var width = 600;
        var rect = new Rectangle(0, 0, width, 48);
        Graphics.DrawRectangleRec(ToRaylibRect(rect), Theme.Panel);
        Graphics.DrawRectangleLinesEx(ToRaylibRect(rect), 1, Theme.Border);
        // Player name
        Graphics.DrawText(PlayerName, 16, 12, Theme.HeaderFont, Theme.Text);
        // Resources
        int x = 180;
        DrawResource(x, 12, "Credits", Credits, Theme.Accent);
        DrawResource(x + 120, 12, "Materials", Materials, Theme.Success);
        DrawResource(x + 240, 12, "Fuel", Fuel, Theme.Info);
        // Turn
        Graphics.DrawText($"Turn: {Turn}", x + 360, 12, Theme.BodyFont, Theme.TextSecondary);
        // Game speed
        Graphics.DrawText($"Speed: {GameSpeed:F1}x", x + 460, 12, Theme.BodyFont, Theme.TextSecondary);
    }

    private void DrawResource(int x, int y, string label, int value, Color color)
    {
        var iconRect = new Rectangle(x, y + 4, 16, 16);
        Graphics.DrawRectangleRec(ToRaylibRect(iconRect), color);
        Graphics.DrawText(label, x + 24, y, Theme.SmallFont, Theme.TextSecondary);
        Graphics.DrawText(value.ToString(), x + 24, y + 14, Theme.BodyFont, Theme.Text);
    }

    public bool HandleInput(IInputState inputState) => false;
    public bool Contains(Vector2 point) => false;
} 