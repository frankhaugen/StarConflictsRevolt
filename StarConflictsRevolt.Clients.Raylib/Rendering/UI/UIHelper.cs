using System.Numerics;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Fonts;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Windowing;
using StarConflictsRevolt.Clients.Models;

namespace StarConflictsRevolt.Clients.Raylib.Rendering.UI;

public static class UIHelper
{
    private static readonly Font _uiFont = FontHelper.Spaceman;
    
    public static void DrawPanel(int x, int y, int width, int height, Color? backgroundColor = null, Color? borderColor = null)
    {
        var bg = backgroundColor ?? Colors.Panel;
        var border = borderColor ?? Colors.Light;

        Graphics.DrawRectangle(x, y, width, height, bg);
        Graphics.DrawRectangleLines(x, y, width, height, border);
    }

    public static void DrawText(string text, int x, int y, int fontSize, Color color, bool centered = false)
    {
        // Prevent crashes from null or empty text
        if (string.IsNullOrEmpty(text)) return;

        try
        {
            Graphics.DrawTextEx(_uiFont, text, new Vector2(x, y), fontSize, 1f, color);
        }
        catch (Exception e)
        {
            // Log the error but don't crash the application
            Console.Error.WriteLine($"Error drawing text '{text}' at ({x}, {y}): {e.Message}");
        }
    }

    public static bool DrawButton(string text, int x, int y, int width, int height, Color? backgroundColor = null, Color? textColor = null)
    {
        var bg = backgroundColor ?? Colors.Primary;
        var textCol = textColor ?? Color.White;
        var mouse = Input.GetMousePosition();

        var isHovered = mouse.X >= x && mouse.X <= x + width && mouse.Y >= y && mouse.Y <= y + height;
        var isPressed = isHovered && Input.IsMouseButtonPressed(MouseButton.Left);

        // Draw button background
        var buttonColor = isPressed ? Color.DarkGray : isHovered ? Color.Gray : bg;
        Graphics.DrawRectangle(x, y, width, height, buttonColor);
        Graphics.DrawRectangleLines(x, y, width, height, Colors.Light);

        // Draw button text
        if (!string.IsNullOrEmpty(text))
        {
            var textX = x + 10; // Simple left alignment for now
            var textY = y + (height - FontSizes.Medium) / 2;
            DrawText(text, textX, textY, FontSizes.Medium, textCol);
        }

        return isPressed;
    }

    public static string DrawTextInput(string currentText, int x, int y, int width, int height, string placeholder = "", int maxLength = 50)
    {
        var mouse = Input.GetMousePosition();
        var isFocused = mouse.X >= x && mouse.X <= x + width && mouse.Y >= y && mouse.Y <= y + height;

        // Draw input background
        var bgColor = isFocused ? Color.White : Colors.Light;
        Graphics.DrawRectangle(x, y, width, height, bgColor);
        Graphics.DrawRectangleLines(x, y, width, height, Colors.Dark);

        // Handle input
        if (isFocused)
        {
            var key = Input.GetCharPressed();
            if (key > 0 && currentText.Length < maxLength) currentText += (char)key;

            // Handle Backspace - only if there's text to delete
            if (currentText.Length > 0)
                // Check if Backspace was pressed this frame
                if (Input.IsKeyPressed(KeyboardKey.Backspace))
                    currentText = currentText[..^1];
        }

        // Draw text or placeholder
        var displayText = string.IsNullOrEmpty(currentText) ? placeholder : currentText;
        var textColor = string.IsNullOrEmpty(currentText) ? Color.Gray : Color.Black;
        if (!string.IsNullOrEmpty(displayText)) UIHelper.DrawText(displayText, x + 5, y + (height - FontSizes.Medium) / 2, FontSizes.Medium, textColor);

        // Draw cursor if focused
        if (isFocused && DateTime.UtcNow.Millisecond / 500 % 2 == 0)
        {
            var cursorX = x + 5 + currentText.Length * 8; // Approximate character width
            Graphics.DrawLine(cursorX, y + 5, cursorX, y + height - 5, Color.Black);
        }

        return currentText;
    }

    public static bool DrawConfirmationDialog(string message, string confirmText = "Yes", string cancelText = "No")
    {
        var screenWidth = Window.GetScreenWidth();
        var screenHeight = Window.GetScreenHeight();

        var dialogWidth = 400;
        var dialogHeight = 200;
        var dialogX = (screenWidth - dialogWidth) / 2;
        var dialogY = (screenHeight - dialogHeight) / 2;

        // Draw overlay
        Graphics.DrawRectangle(0, 0, screenWidth, screenHeight, new Color(0, 0, 0, 128));

        // Draw dialog
        DrawPanel(dialogX, dialogY, dialogWidth, dialogHeight, Colors.Background, Colors.Light);

        // Draw message
        if (!string.IsNullOrEmpty(message)) DrawText(message, dialogX + dialogWidth / 2, dialogY + 40, FontSizes.Medium, Color.White, true);

        // Draw buttons
        var buttonWidth = 80;
        var buttonHeight = 30;
        var buttonY = dialogY + dialogHeight - 50;

        var confirmX = dialogX + dialogWidth / 2 - buttonWidth - 10;
        var cancelX = dialogX + dialogWidth / 2 + 10;

        var confirmed = DrawButton(confirmText, confirmX, buttonY, buttonWidth, buttonHeight, Colors.Success);
        var cancelled = DrawButton(cancelText, cancelX, buttonY, buttonWidth, buttonHeight, Colors.Danger);

        return confirmed || cancelled;
    }

    public static void DrawStatusBar(int y, string status, Color? backgroundColor = null)
    {
        var screenWidth = Window.GetScreenWidth();
        var height = 30;
        var bg = backgroundColor ?? Colors.Dark;

        Graphics.DrawRectangle(0, y, screenWidth, height, bg);
        Graphics.DrawRectangleLines(0, y, screenWidth, height, Colors.Light);
        if (!string.IsNullOrEmpty(status)) UIHelper.DrawText(status, 10, y + 5, FontSizes.Small, Color.White);
    }

    public static void DrawInfoPanel(int x, int y, int width, int height, string title, List<(string Label, string Value)> info)
    {
        DrawPanel(x, y, width, height);

        // Draw title
        if (!string.IsNullOrEmpty(title)) DrawText(title, x + 10, y + 10, FontSizes.Large, Color.White);

        // Draw info items
        var infoY = y + 40;
        foreach (var (label, value) in info)
        {
            var infoText = $"{label}: {value}";
            if (!string.IsNullOrEmpty(infoText)) DrawText(infoText, x + 10, infoY, FontSizes.Small, Color.White);
            infoY += 20;
        }
    }

    public static class Colors
    {
        public static Color Primary = new(52, 152, 219, 255);
        public static Color Secondary = new(155, 89, 182, 255);
        public static Color Success = new(46, 204, 113, 255);
        public static Color Warning = new(241, 196, 15, 255);
        public static Color Danger = new(231, 76, 60, 255);
        public static Color Dark = new(44, 62, 80, 255);
        public static Color Light = new(236, 240, 241, 255);
        public static Color Background = new(26, 26, 26, 255);
        public static Color Panel = new(52, 73, 94, 255);
        public static Color Accent = new(255, 195, 0, 255); // Sci-fi yellow/orange
    }

    public static class FontSizes
    {
        public const int Small = 16;
        public const int Medium = 20;
        public const int Large = 24;
        public const int Title = 32;
    }

    public static class SciFiColors
    {
        public static Color LaserRay = Color.Red;
        public static Color Asteroid = Color.Gray;
        public static Color Background = Color.Black;
        public static Color HUDText = Color.Green;
        public static Color HUDBackground = new(10, 10, 20, 220);
        public static Color HUDSeperator = Color.DarkGray;
        public static Color Reticle = Color.Green;
        public static Color GameOverText = Color.Red;
        public static Color Star = Color.White;
        public static Color MinimapBackground = Color.Black;
        public static Color MinimapGrid = Color.DarkGray;
        public static Color RadarObject = Color.Green;
        public static Color EnergyBar = Color.Green;
        public static Color ShieldBar = Color.Blue;
        public static Color HullBar = Color.Red;
        public static Color MinimapShip = Color.Green;
    }

    public static void DrawReticle()
    {
        var centerX = Window.GetScreenWidth() / 2;
        var centerY = Window.GetScreenHeight() / 2;
        var size = 10;
        Graphics.DrawLine(centerX - size, centerY, centerX + size, centerY, SciFiColors.Reticle);
        Graphics.DrawLine(centerX, centerY - size, centerX, centerY + size, SciFiColors.Reticle);
    }

    public static void DrawResourceBars(int x, int y, int width, int barHeight, GameStateInfoDto? playerState)
    {
        // Energy (Credits)
        DrawBar(x, y, width, barHeight, playerState?.Credits ?? 0, 1000, SciFiColors.EnergyBar, "Credits");
        // Shield (Materials)
        DrawBar(x, y + barHeight + 4, width, barHeight, playerState?.Materials ?? 0, 500, SciFiColors.ShieldBar, "Materials");
        // Hull (Fuel)
        DrawBar(x, y + 2 * (barHeight + 4), width, barHeight, playerState?.Fuel ?? 0, 200, SciFiColors.HullBar, "Fuel");
    }

    private static void DrawBar(int x, int y, int width, int height, int value, int max, Color color, string label)
    {
        Graphics.DrawRectangle(x, y, width, height, SciFiColors.HUDSeperator);
        int filled = (int)(width * (Math.Min(value, max) / (float)max));
        Graphics.DrawRectangle(x, y, filled, height, color);
        DrawText($"{label}: {value}", x + 8, y + 2, FontSizes.Small, SciFiColors.HUDText);
    }

    public static void DrawResourceBar(int x, int y, int width, int height, GameStateInfoDto? playerState)
    {
        // Draw background panel
        DrawPanel(x, y, width, height, SciFiColors.HUDBackground, SciFiColors.HUDSeperator);
        // Draw resource bars
        DrawResourceBars(x + 16, y + 8, width - 32, 14, playerState);
    }

    public static void DrawMinimap(int x, int y, int width, int height, WorldDto? world)
    {
        // Draw minimap background
        Graphics.DrawRectangle(x, y, width, height, SciFiColors.MinimapBackground);
        Graphics.DrawRectangleLines(x, y, width, height, SciFiColors.MinimapGrid);
        DrawText("Minimap", x + 5, y + 5, FontSizes.Small, SciFiColors.HUDText);
        if (world?.Galaxy?.StarSystems == null) return;
        var systems = world.Galaxy.StarSystems.ToList();
        if (systems.Count == 0) return;
        var minX = systems.Min(s => s.Coordinates.X);
        var maxX = systems.Max(s => s.Coordinates.X);
        var minY = systems.Min(s => s.Coordinates.Y);
        var maxY = systems.Max(s => s.Coordinates.Y);
        var scaleX = (width - 20) / (maxX - minX);
        var scaleY = (height - 40) / (maxY - minY);
        var scale = Math.Min(scaleX, scaleY);
        foreach (var system in systems)
        {
            var mapX = x + 10 + (int)((system.Coordinates.X - minX) * scale);
            var mapY = y + 25 + (int)((system.Coordinates.Y - minY) * scale);
            Graphics.DrawCircle(mapX, mapY, 2, SciFiColors.Star);
        }
    }

    /// <summary>
    /// Draws a sci-fi style border around a rectangle.
    /// </summary>
    public static void DrawSciFiBorder(int x, int y, int width, int height, Color borderColor, int thickness = 2)
    {
        // Draw main rectangle
        Graphics.DrawRectangleLines(x, y, width, height, borderColor);
        // Draw angular corners
        int cornerLen = 24;
        // Top-left
        Graphics.DrawLine(x, y, x + cornerLen, y, borderColor);
        Graphics.DrawLine(x, y, x, y + cornerLen, borderColor);
        // Top-right
        Graphics.DrawLine(x + width, y, x + width - cornerLen, y, borderColor);
        Graphics.DrawLine(x + width, y, x + width, y + cornerLen, borderColor);
        // Bottom-left
        Graphics.DrawLine(x, y + height, x + cornerLen, y + height, borderColor);
        Graphics.DrawLine(x, y + height, x, y + height - cornerLen, borderColor);
        // Bottom-right
        Graphics.DrawLine(x + width, y + height, x + width - cornerLen, y + height, borderColor);
        Graphics.DrawLine(x + width, y + height, x + width, y + height - cornerLen, borderColor);
    }

    /// <summary>
    /// Draws a sci-fi style circular widget (e.g., for HUD indicators).
    /// </summary>
    public static void DrawSciFiCircle(int centerX, int centerY, int radius, Color borderColor, float percent = 1.0f, Color? fillColor = null)
    {
        // Draw outer circle
        Graphics.DrawCircleLines(centerX, centerY, radius, borderColor);
        // Draw progress arc if percent < 1
        if (percent < 1.0f)
        {
            int segments = 64;
            int filledSegments = (int)(segments * percent);
            for (int i = 0; i < filledSegments; i++)
            {
                float angle1 = (float)(2 * Math.PI * i / segments);
                float angle2 = (float)(2 * Math.PI * (i + 1) / segments);
                int x1 = centerX + (int)(radius * Math.Cos(angle1));
                int y1 = centerY + (int)(radius * Math.Sin(angle1));
                int x2 = centerX + (int)(radius * Math.Cos(angle2));
                int y2 = centerY + (int)(radius * Math.Sin(angle2));
                Graphics.DrawLine(x1, y1, x2, y2, fillColor ?? borderColor);
            }
        }
        // Draw inner details (tick marks)
        for (int i = 0; i < 8; i++)
        {
            float angle = (float)(2 * Math.PI * i / 8);
            int x1 = centerX + (int)((radius - 4) * Math.Cos(angle));
            int y1 = centerY + (int)((radius - 4) * Math.Sin(angle));
            int x2 = centerX + (int)((radius - 12) * Math.Cos(angle));
            int y2 = centerY + (int)((radius - 12) * Math.Sin(angle));
            Graphics.DrawLine(x1, y1, x2, y2, borderColor);
        }
    }

    public static void DrawPlanetIcon(int x, int y, int radius = 3)
    {
        Graphics.DrawCircle(x, y, radius, Color.Blue);
        Graphics.DrawCircleLines(x, y, radius + 1, Color.LightGray);
    }
    public static void DrawFleetIcon(int x, int y)
    {
        Graphics.DrawRectangle(x, y, 6, 3, Color.LightGray);
        Graphics.DrawRectangleLines(x, y, 6, 3, Color.DarkGray);
    }
}

public static class FontHelper
{
    private const int FNT = 48; // Default font size, can be adjusted as needed
    private const string FontPath = "Assets/Fonts/";
    
    private static string GetPath(string fileName) => Path.Combine(FontPath, fileName);

    
    public static Font Galaxy { get; private set; } = Font.LoadEx(GetPath("Galaxy.ttf"), FNT, ReadOnlySpan<int>.Empty);
    
    public static Font Spaceman { get; private set; } = Font.LoadEx(GetPath("Spaceman.ttf"), FNT, ReadOnlySpan<int>.Empty);
    
    public static Font NeuropolX { get; private set; } = Font.LoadEx(GetPath("NeuropolX.otf"), FNT, ReadOnlySpan<int>.Empty);
}