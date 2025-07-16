using Bliss.CSharp.Fonts;
using Bliss.CSharp.Graphics.Rendering.Batches.Sprites;
using Bliss.CSharp.Transformations;
using System.Numerics;
using RectangleF = Bliss.CSharp.Transformations.RectangleF;
using Color = Bliss.CSharp.Colors.Color;
using Veldrid;

namespace StarConflictsRevolt.Clients.Bliss.Core.UI;

/// <summary>
/// Service for handling text rendering using Bliss.CSharp font system.
/// Manages font loading and provides text rendering capabilities.
/// </summary>
public class TextRenderingService : IDisposable
{
    private readonly Dictionary<string, Font> _fonts = new();
    private readonly Dictionary<string, float> _defaultFontSizes = new();
    private bool _disposed = false;
    
    public TextRenderingService()
    {
        LoadDefaultFonts();
    }
    
    /// <summary>
    /// Loads the default fonts used by the UI system.
    /// </summary>
    private void LoadDefaultFonts()
    {
        try
        {
            // Load the Galaxy font for UI elements
            var galaxyFont = new Font("Assets/Fonts/Galaxy.ttf");
            _fonts["Galaxy"] = galaxyFont;
            _defaultFontSizes["Galaxy"] = 24f;
            
            // Add more fonts as needed
            // _fonts["Arial"] = new Font("Assets/Fonts/arial.ttf");
            // _defaultFontSizes["Arial"] = 16f;
        }
        catch (Exception ex)
        {
            // Log error but don't crash - we'll use fallback rendering
            Console.WriteLine($"Warning: Could not load fonts: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Renders text using the specified font and parameters.
    /// </summary>
    /// <param name="spriteBatch">The sprite batch to render with</param>
    /// <param name="text">The text to render</param>
    /// <param name="position">The position to render at</param>
    /// <param name="fontName">The name of the font to use</param>
    /// <param name="fontSize">The size of the font</param>
    /// <param name="color">The color of the text</param>
    /// <param name="alignment">Text alignment (default: left)</param>
    public void DrawText(SpriteBatch spriteBatch, string text, Vector2 position, 
        CommandList commandList, OutputDescription outputDescription,
        string fontName = "Galaxy", float? fontSize = null, Color? color = null, 
        TextAlignment alignment = TextAlignment.Left)
    {
        if (string.IsNullOrEmpty(text) || !_fonts.ContainsKey(fontName))
            return;
            
        var font = _fonts[fontName];
        var size = fontSize ?? _defaultFontSizes[fontName];
        var textColor = color ?? Color.White;
        
        // Calculate text bounds for alignment
        var textBounds = GetTextBounds(font, text, size);
        var alignedPosition = CalculateAlignedPosition(position, textBounds, alignment);
        
        spriteBatch.Begin(commandList, outputDescription);
        spriteBatch.DrawText(font, text, alignedPosition, size, color: textColor);
        spriteBatch.End();
    }
    
    /// <summary>
    /// Renders text centered within a rectangle.
    /// </summary>
    /// <param name="spriteBatch">The sprite batch to render with</param>
    /// <param name="text">The text to render</param>
    /// <param name="bounds">The rectangle to center the text within</param>
    /// <param name="fontName">The name of the font to use</param>
    /// <param name="fontSize">The size of the font</param>
    /// <param name="color">The color of the text</param>
    public void DrawTextCentered(SpriteBatch spriteBatch, string text, RectangleF bounds,
        CommandList commandList, OutputDescription outputDescription,
        string fontName = "Galaxy", float? fontSize = null, Color? color = null)
    {
        if (string.IsNullOrEmpty(text) || !_fonts.ContainsKey(fontName))
            return;
            
        var font = _fonts[fontName];
        var size = fontSize ?? _defaultFontSizes[fontName];
        var textColor = color ?? Color.White;
        
        // Calculate text bounds
        var textBounds = GetTextBounds(font, text, size);
        
        // Center the text within the bounds
        var centerX = bounds.X + (bounds.Width - textBounds.Width) / 2f;
        var centerY = bounds.Y + (bounds.Height - textBounds.Height) / 2f;
        var centerPosition = new Vector2(centerX, centerY);
        
        spriteBatch.Begin(commandList, outputDescription);
        spriteBatch.DrawText(font, text, centerPosition, size, color: textColor);
        spriteBatch.End();
    }
    
    /// <summary>
    /// Gets the bounds of a text string when rendered with the specified font and size.
    /// </summary>
    /// <param name="font">The font to measure with</param>
    /// <param name="text">The text to measure</param>
    /// <param name="fontSize">The font size</param>
    /// <returns>The bounds of the text</returns>
    private RectangleF GetTextBounds(Font font, string text, float fontSize)
    {
        try
        {
            var spriteFont = font.GetSpriteFont(fontSize);
            var size = spriteFont.MeasureString(text);
            return new RectangleF(0, 0, size.X, size.Y);
        }
        catch
        {
            // Fallback: estimate bounds based on character count and font size
            var estimatedWidth = text.Length * fontSize * 0.6f; // Rough estimate
            var estimatedHeight = fontSize * 1.2f; // Rough estimate
            return new RectangleF(0, 0, estimatedWidth, estimatedHeight);
        }
    }
    
    /// <summary>
    /// Calculates the position for aligned text.
    /// </summary>
    /// <param name="basePosition">The base position</param>
    /// <param name="textBounds">The bounds of the text</param>
    /// <param name="alignment">The alignment type</param>
    /// <returns>The aligned position</returns>
    private Vector2 CalculateAlignedPosition(Vector2 basePosition, RectangleF textBounds, TextAlignment alignment)
    {
        return alignment switch
        {
            TextAlignment.Left => basePosition,
            TextAlignment.Center => new Vector2(basePosition.X - textBounds.Width / 2f, basePosition.Y),
            TextAlignment.Right => new Vector2(basePosition.X - textBounds.Width, basePosition.Y),
            _ => basePosition
        };
    }
    
    /// <summary>
    /// Gets a font by name.
    /// </summary>
    /// <param name="fontName">The name of the font</param>
    /// <returns>The font, or null if not found</returns>
    public Font? GetFont(string fontName)
    {
        return _fonts.TryGetValue(fontName, out var font) ? font : null;
    }
    
    /// <summary>
    /// Gets the default font size for a font.
    /// </summary>
    /// <param name="fontName">The name of the font</param>
    /// <returns>The default font size, or 16 if not found</returns>
    public float GetDefaultFontSize(string fontName)
    {
        return _defaultFontSizes.TryGetValue(fontName, out var size) ? size : 16f;
    }
    
    public void Dispose()
    {
        if (!_disposed)
        {
            foreach (var font in _fonts.Values)
            {
                font?.Dispose();
            }
            _fonts.Clear();
            _defaultFontSizes.Clear();
            _disposed = true;
        }
    }
}

 