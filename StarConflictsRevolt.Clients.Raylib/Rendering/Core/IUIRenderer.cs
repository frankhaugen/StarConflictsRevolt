using System.Numerics;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Camera.Cam2D;

namespace StarConflictsRevolt.Clients.Raylib.Rendering.Core;

/// <summary>
/// Abstract renderer interface for UI elements to make rendering testable.
/// </summary>
public interface IUIRenderer
{
    /// <summary>
    /// Screen width in pixels.
    /// </summary>
    int ScreenWidth { get; }
    
    /// <summary>
    /// Screen height in pixels.
    /// </summary>
    int ScreenHeight { get; }
    
    /// <summary>
    /// Current camera for coordinate transformations.
    /// </summary>
    Camera2D Camera { get; }
    
    /// <summary>
    /// Draw a rectangle.
    /// </summary>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    /// <param name="color">Color.</param>
    void DrawRectangle(int x, int y, int width, int height, Color color);
    
    /// <summary>
    /// Draw rectangle outline.
    /// </summary>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    /// <param name="color">Color.</param>
    void DrawRectangleLines(int x, int y, int width, int height, Color color);
    
    /// <summary>
    /// Draw text.
    /// </summary>
    /// <param name="text">Text to draw.</param>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    /// <param name="fontSize">Font size.</param>
    /// <param name="color">Color.</param>
    void DrawText(string text, int x, int y, int fontSize, Color color);
    
    /// <summary>
    /// Draw text with centering option.
    /// </summary>
    /// <param name="text">Text to draw.</param>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    /// <param name="fontSize">Font size.</param>
    /// <param name="color">Color.</param>
    /// <param name="centered">Whether to center the text.</param>
    void DrawText(string text, int x, int y, int fontSize, Color color, bool centered);
    
    /// <summary>
    /// Draw a circle.
    /// </summary>
    /// <param name="x">Center X coordinate.</param>
    /// <param name="y">Center Y coordinate.</param>
    /// <param name="radius">Radius.</param>
    /// <param name="color">Color.</param>
    void DrawCircle(int x, int y, float radius, Color color);
    
    /// <summary>
    /// Draw a line.
    /// </summary>
    /// <param name="startX">Start X coordinate.</param>
    /// <param name="startY">Start Y coordinate.</param>
    /// <param name="endX">End X coordinate.</param>
    /// <param name="endY">End Y coordinate.</param>
    /// <param name="color">Color.</param>
    void DrawLine(int startX, int startY, int endX, int endY, Color color);
    
    /// <summary>
    /// Draw a pixel.
    /// </summary>
    /// <param name="position">Position.</param>
    /// <param name="color">Color.</param>
    void DrawPixel(Vector2 position, Color color);
    
    /// <summary>
    /// Measure text width.
    /// </summary>
    /// <param name="text">Text to measure.</param>
    /// <param name="fontSize">Font size.</param>
    /// <returns>Text width in pixels.</returns>
    int MeasureText(string text, int fontSize);
    
    /// <summary>
    /// Begin 2D camera mode.
    /// </summary>
    void BeginMode2D();
    
    /// <summary>
    /// End 2D camera mode.
    /// </summary>
    void EndMode2D();
    
    /// <summary>
    /// Clear the screen with a background color.
    /// </summary>
    /// <param name="color">Background color.</param>
    void ClearBackground(Color color);
    
    /// <summary>
    /// Draw a panel with optional background and border colors.
    /// </summary>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    /// <param name="backgroundColor">Background color.</param>
    /// <param name="borderColor">Border color.</param>
    void DrawPanel(int x, int y, int width, int height, Color? backgroundColor = null, Color? borderColor = null);
    
    /// <summary>
    /// Draw a button and return whether it was clicked.
    /// </summary>
    /// <param name="text">Button text.</param>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    /// <param name="backgroundColor">Background color.</param>
    /// <param name="textColor">Text color.</param>
    /// <returns>True if button was clicked.</returns>
    bool DrawButton(string text, int x, int y, int width, int height, Color? backgroundColor = null, Color? textColor = null);
    
    /// <summary>
    /// Draw a text input field and return the current text.
    /// </summary>
    /// <param name="currentText">Current text value.</param>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    /// <param name="placeholder">Placeholder text.</param>
    /// <param name="maxLength">Maximum text length.</param>
    /// <returns>Updated text value.</returns>
    string DrawTextInput(string currentText, int x, int y, int width, int height, string placeholder = "", int maxLength = 50);
    
    /// <summary>
    /// Draw a status bar.
    /// </summary>
    /// <param name="y">Y coordinate.</param>
    /// <param name="status">Status text.</param>
    /// <param name="backgroundColor">Background color.</param>
    void DrawStatusBar(int y, string status, Color? backgroundColor = null);
} 