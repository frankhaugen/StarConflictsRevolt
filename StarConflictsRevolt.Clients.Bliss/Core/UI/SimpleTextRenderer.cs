using Bliss.CSharp.Fonts;
using Bliss.CSharp.Graphics.Rendering.Batches.Sprites;
using Bliss.CSharp.Transformations;
using System.Numerics;
using Veldrid;
using Color = Bliss.CSharp.Colors.Color;
using RectangleF = Bliss.CSharp.Transformations.RectangleF;

namespace StarConflictsRevolt.Clients.Bliss.Core.UI;

/// <summary>
/// Simplified text renderer that uses the ResourceManager.
/// Follows the Facade pattern to hide complexity.
/// </summary>
public class SimpleTextRenderer
{
    private readonly ResourceManager _resourceManager;
    private readonly Dictionary<string, float> _defaultFontSizes = new()
    {
        ["Galaxy"] = 24f
    };
    
    public SimpleTextRenderer(ResourceManager resourceManager)
    {
        _resourceManager = resourceManager ?? throw new ArgumentNullException(nameof(resourceManager));
    }
    
    /// <summary>
    /// Draws text at the specified position.
    /// </summary>
    public void DrawText(string text, Vector2 position, string fontName = "Galaxy", float? fontSize = null, Color? color = null)
    {
        if (string.IsNullOrEmpty(text)) return;
        
        // Ensure resources are initialized
        _resourceManager.Initialize();
        
        var font = _resourceManager.GetFont(fontName);
        if (font == null) return;
        
        var size = fontSize ?? _defaultFontSizes.GetValueOrDefault(fontName, 16f);
        var textColor = color ?? Color.White;
        
        // Note: This method requires the SpriteBatch to be in the correct state
        // Use DrawText(SpriteBatch spriteBatch, ...) instead for better control
        _resourceManager.SpriteBatch.DrawText(font, text, position, size, color: textColor);
    }
    
    /// <summary>
    /// Draws text centered within a rectangle.
    /// </summary>
    public void DrawTextCentered(string text, RectangleF bounds, string fontName = "Galaxy", float? fontSize = null, Color? color = null)
    {
        if (string.IsNullOrEmpty(text)) return;
        
        // Ensure resources are initialized
        _resourceManager.Initialize();
        
        var font = _resourceManager.GetFont(fontName);
        if (font == null) return;
        
        var size = fontSize ?? _defaultFontSizes.GetValueOrDefault(fontName, 16f);
        var textColor = color ?? Color.White;
        
        // Calculate center position
        var centerX = bounds.X + bounds.Width / 2f;
        var centerY = bounds.Y + bounds.Height / 2f;
        var centerPosition = new Vector2(centerX, centerY);
        
        // Use the current SpriteBatch from ResourceManager
        _resourceManager.SpriteBatch.DrawText(font, text, centerPosition, size, color: textColor);
    }
    
    /// <summary>
    /// Draws text centered within a rectangle using the provided SpriteBatch.
    /// </summary>
    public void DrawTextCentered(string text, RectangleF bounds, SpriteBatch spriteBatch, string fontName = "Galaxy", float? fontSize = null, Color? color = null)
    {
        if (string.IsNullOrEmpty(text)) return;
        
        // Ensure resources are initialized
        _resourceManager.Initialize();
        
        var font = _resourceManager.GetFont(fontName);
        if (font == null) return;
        
        var size = fontSize ?? _defaultFontSizes.GetValueOrDefault(fontName, 16f);
        var textColor = color ?? Color.White;
        
        // Calculate center position
        var centerX = bounds.X + bounds.Width / 2f;
        var centerY = bounds.Y + bounds.Height / 2f;
        var centerPosition = new Vector2(centerX, centerY);
        
        // Use the provided SpriteBatch
        spriteBatch.DrawText(font, text, centerPosition, size, color: textColor);
    }
    
    /// <summary>
    /// Draws text with custom alignment.
    /// </summary>
    public void DrawTextAligned(string text, Vector2 position, TextAlignment alignment, string fontName = "Galaxy", float? fontSize = null, Color? color = null)
    {
        if (string.IsNullOrEmpty(text)) return;
        
        // Ensure resources are initialized
        _resourceManager.Initialize();
        
        var font = _resourceManager.GetFont(fontName);
        if (font == null) return;
        
        var size = fontSize ?? _defaultFontSizes.GetValueOrDefault(fontName, 16f);
        var textColor = color ?? Color.White;
        
        // For now, just draw at position - alignment can be added later if needed
        _resourceManager.SpriteBatch.DrawText(font, text, position, size, color: textColor);
    }
}

/// <summary>
/// Text alignment options.
/// </summary>
public enum TextAlignment
{
    Left,
    Center,
    Right
} 