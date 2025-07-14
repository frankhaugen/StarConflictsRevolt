using System.Numerics;
using Raylib_CSharp.Camera.Cam2D;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Windowing;
using StarConflictsRevolt.Clients.Raylib.Rendering.UI;

namespace StarConflictsRevolt.Clients.Raylib.Rendering.Core;

/// <summary>
/// Raylib implementation of IUIRenderer.
/// </summary>
public class RaylibUIRenderer : IUIRenderer
{
    public int ScreenWidth => Window.GetScreenWidth();
    public int ScreenHeight => Window.GetScreenHeight();
    public Camera2D Camera { get; set; } = new Camera2D();
    
    public void DrawRectangle(int x, int y, int width, int height, Color color)
    {
        Graphics.DrawRectangle(x, y, width, height, color);
    }
    
    public void DrawRectangleLines(int x, int y, int width, int height, Color color)
    {
        Graphics.DrawRectangleLines(x, y, width, height, color);
    }
    
    public void DrawText(string text, int x, int y, int fontSize, Color color)
    {
        UIHelper.DrawText(text, x, y, fontSize, color);
    }
    
    public void DrawText(string text, int x, int y, int fontSize, Color color, bool centered)
    {
        // For now, just call the non-centered version
        // TODO: Implement proper text centering
        DrawText(text, x, y, fontSize, color);
    }
    
    public void DrawCircle(int x, int y, float radius, Color color)
    {
        Graphics.DrawCircle(x, y, radius, color);
    }
    
    public void DrawLine(int startX, int startY, int endX, int endY, Color color)
    {
        Graphics.DrawLine(startX, startY, endX, endY, color);
    }
    
    public void DrawPixel(Vector2 position, Color color)
    {
        Graphics.DrawPixel((int)position.X, (int)position.Y, color);
    }
    
    public int MeasureText(string text, int fontSize)
    {
        // TODO: Implement proper text measurement
        return text.Length * fontSize / 2; // Rough approximation
    }
    
    public void BeginMode2D()
    {
        Graphics.BeginMode2D(Camera);
    }
    
    public void EndMode2D()
    {
        Graphics.EndMode2D();
    }
    
    public void ClearBackground(Color color)
    {
        Graphics.ClearBackground(color);
    }
    
    public void DrawPanel(int x, int y, int width, int height, Color? backgroundColor = null, Color? borderColor = null)
    {
        var bg = backgroundColor ?? UIHelper.Colors.Panel;
        var border = borderColor ?? UIHelper.Colors.Light;
        
        Graphics.DrawRectangle(x, y, width, height, bg);
        Graphics.DrawRectangleLines(x, y, width, height, border);
    }
    
    public bool DrawButton(string text, int x, int y, int width, int height, Color? backgroundColor = null, Color? textColor = null)
    {
        var bg = backgroundColor ?? UIHelper.Colors.Primary;
        var textCol = textColor ?? Color.White;
        var mouse = Input.GetMousePosition();
        
        var isHovered = mouse.X >= x && mouse.X <= x + width && mouse.Y >= y && mouse.Y <= y + height;
        var isPressed = isHovered && Input.IsMouseButtonPressed(MouseButton.Left);
        
        // Draw button background
        var buttonColor = isPressed ? Color.DarkGray : isHovered ? Color.Gray : bg;
        Graphics.DrawRectangle(x, y, width, height, buttonColor);
        Graphics.DrawRectangleLines(x, y, width, height, UIHelper.Colors.Light);
        
        // Draw button text
        if (!string.IsNullOrEmpty(text))
        {
            var textX = x + 10; // Simple left alignment for now
            var textY = y + (height - UIHelper.FontSizes.Medium) / 2;
            DrawText(text, textX, textY, UIHelper.FontSizes.Medium, textCol);
        }
        
        return isPressed;
    }
    
    public string DrawTextInput(string currentText, int x, int y, int width, int height, string placeholder = "", int maxLength = 50)
    {
        var mouse = Input.GetMousePosition();
        var isFocused = mouse.X >= x && mouse.X <= x + width && mouse.Y >= y && mouse.Y <= y + height;
        
        // Draw input background
        var bgColor = isFocused ? Color.White : UIHelper.Colors.Light;
        Graphics.DrawRectangle(x, y, width, height, bgColor);
        Graphics.DrawRectangleLines(x, y, width, height, UIHelper.Colors.Dark);
        
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
        if (!string.IsNullOrEmpty(displayText)) UIHelper.DrawText(displayText, x + 5, y + (height - UIHelper.FontSizes.Medium) / 2, UIHelper.FontSizes.Medium, textColor);
        
        // Draw cursor if focused
        if (isFocused && DateTime.UtcNow.Millisecond / 500 % 2 == 0)
        {
            var cursorX = x + 5 + currentText.Length * 8; // Approximate character width
            Graphics.DrawLine(cursorX, y + 5, cursorX, y + height - 5, Color.Black);
        }
        
        return currentText;
    }
    
    public void DrawStatusBar(int y, string status, Color? backgroundColor = null)
    {
        var screenWidth = Window.GetScreenWidth();
        var height = 30;
        var bg = backgroundColor ?? UIHelper.Colors.Dark;
        
        Graphics.DrawRectangle(0, y, screenWidth, height, bg);
        Graphics.DrawRectangleLines(0, y, screenWidth, height, UIHelper.Colors.Light);
        if (!string.IsNullOrEmpty(status)) UIHelper.DrawText(status, 10, y + 5, UIHelper.FontSizes.Small, Color.White);
    }
} 