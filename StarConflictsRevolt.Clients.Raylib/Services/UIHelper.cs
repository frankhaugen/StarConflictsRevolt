using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Windowing;
using StarConflictsRevolt.Clients.Models;

namespace StarConflictsRevolt.Clients.Raylib.Services;

public static class UIHelper
{
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

        // TODO: Implement proper text centering when MeasureText is available
        Graphics.DrawText(text, x, y, fontSize, color);
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
            Graphics.DrawText(text, textX, textY, FontSizes.Medium, textCol);
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
            {
                // Check if Backspace was pressed this frame
                if (Input.IsKeyPressed(KeyboardKey.Backspace))
                {
                    currentText = currentText[..^1];
                }
            }
        }

        // Draw text or placeholder
        var displayText = string.IsNullOrEmpty(currentText) ? placeholder : currentText;
        var textColor = string.IsNullOrEmpty(currentText) ? Color.Gray : Color.Black;
        if (!string.IsNullOrEmpty(displayText)) Graphics.DrawText(displayText, x + 5, y + (height - FontSizes.Medium) / 2, FontSizes.Medium, textColor);

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
        if (!string.IsNullOrEmpty(status)) Graphics.DrawText(status, 10, y + 5, FontSizes.Small, Color.White);
    }

    public static void DrawInfoPanel(int x, int y, int width, int height, string title, List<(string Label, string Value)> info)
    {
        DrawPanel(x, y, width, height);

        // Draw title
        if (!string.IsNullOrEmpty(title)) Graphics.DrawText(title, x + 10, y + 10, FontSizes.Large, Color.White);

        // Draw info items
        var infoY = y + 40;
        foreach (var (label, value) in info)
        {
            var infoText = $"{label}: {value}";
            if (!string.IsNullOrEmpty(infoText)) Graphics.DrawText(infoText, x + 10, infoY, FontSizes.Small, Color.White);
            infoY += 20;
        }
    }

    public static void DrawMinimap(int x, int y, int width, int height, WorldDto? world)
    {
        DrawPanel(x, y, width, height);
        Graphics.DrawText("Minimap", x + 5, y + 5, FontSizes.Small, Color.White);

        if (world?.Galaxy?.StarSystems == null) return;

        var systems = world.Galaxy.StarSystems.ToList();
        if (systems.Count == 0) return;

        // Find bounds
        var minX = systems.Min(s => s.Coordinates.X);
        var maxX = systems.Max(s => s.Coordinates.X);
        var minY = systems.Min(s => s.Coordinates.Y);
        var maxY = systems.Max(s => s.Coordinates.Y);

        var scaleX = (width - 20) / (maxX - minX);
        var scaleY = (height - 40) / (maxY - minY);
        var scale = Math.Min(scaleX, scaleY);

        // Draw systems
        foreach (var system in systems)
        {
            var mapX = x + 10 + (int)((system.Coordinates.X - minX) * scale);
            var mapY = y + 25 + (int)((system.Coordinates.Y - minY) * scale);

            Graphics.DrawCircle(mapX, mapY, 2, Color.Yellow);
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
    }

    public static class FontSizes
    {
        public const int Small = 16;
        public const int Medium = 20;
        public const int Large = 24;
        public const int Title = 32;
    }
}