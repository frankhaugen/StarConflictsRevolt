using Bliss.CSharp.Fonts;
using Bliss.CSharp.Graphics.Rendering.Batches.Primitives;
using Bliss.CSharp.Graphics.Rendering.Batches.Sprites;
using Color = Bliss.CSharp.Colors.Color;
using RectangleF = Bliss.CSharp.Transformations.RectangleF;

namespace StarConflictsRevolt.Clients.Bliss.Core.UI;

/// <summary>
/// Simplified text renderer that uses system default fonts.
/// Follows the Facade pattern to hide complexity.
/// </summary>
public class SimpleTextRenderer : IDisposable
{
    private readonly Dictionary<string, float> _defaultFontSizes = new()
    {
        ["Default"] = 24f
    };
    
    // Cache the loaded font to avoid recreation every frame
    private Font? _cachedFont = null;
    private readonly object _fontLock = new object();
    private bool _disposed = false;
    
    public SimpleTextRenderer()
    {
        // No custom font loading - use system defaults
    }
    
    /// <summary>
    /// Gets or creates a cached font instance.
    /// </summary>
    private Font GetOrCreateFont()
    {
        if (_cachedFont != null)
        {
            return _cachedFont;
        }
        
        lock (_fontLock)
        {
            if (_cachedFont != null)
            {
                return _cachedFont;
            }
            
            // Try multiple font paths in order of preference
            var fontPaths = new[]
            {
                "C:\\Windows\\Fonts\\arial.ttf",
                "C:\\Windows\\Fonts\\calibri.ttf",
                "C:\\Windows\\Fonts\\tahoma.ttf",
                "C:\\Windows\\Fonts\\verdana.ttf",
                "C:\\Windows\\Fonts\\segoeui.ttf"
            };
            
            foreach (var fontPath in fontPaths)
            {
                try 
                { 
                    _cachedFont = new Font(fontPath); 
                    Console.WriteLine($"Successfully loaded font: {fontPath}");
                    return _cachedFont;
                } 
                catch (Exception ex) 
                { 
                    Console.WriteLine($"Failed to load font {fontPath}: {ex.Message}");
                }
            }
            
            // If all system fonts fail, try to create a default font
            try
            {
                // Try to create a font with a simple path that might exist
                _cachedFont = new Font("arial");
                Console.WriteLine("Created default font as fallback");
                return _cachedFont;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create default font: {ex.Message}");
                return null!;
            }
        }
    }
    
    /// <summary>
    /// Draw text centered in the specified bounds using system default font.
    /// </summary>
    public void DrawTextCentered(string text, RectangleF bounds, SpriteBatch spriteBatch, string fontName, float fontSize, Color color)
    {
        try
        {
            var font = GetOrCreateFont();
            
            if (font == null)
            {
                Console.WriteLine("Warning: Could not load any font, text will not be rendered");
                return;
            }
            
            // Calculate text position (centered)
            var spriteFont = font.GetSpriteFont(fontSize);
            var textSize = spriteFont.MeasureString(text);
            var textX = bounds.X + (bounds.Width - textSize.X) / 2;
            var textY = bounds.Y + (bounds.Height - textSize.Y) / 2;
            
            // Draw text using SpriteBatch
            spriteBatch.DrawText(font, text, new Vector2(textX, textY), fontSize, color: color, layerDepth: 1.5f);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error rendering text '{text}': {ex.Message}");
            // Fallback to simple rectangle representation
        }
    }
    
    /// <summary>
    /// Draw text at a specific position.
    /// </summary>
    public void DrawText(string text, Vector2 position, SpriteBatch spriteBatch, string fontName, float fontSize, Color color)
    {
        try
        {
            var font = GetOrCreateFont();
            
            if (font == null)
            {
                Console.WriteLine("Warning: Could not load any font, text will not be rendered");
                return;
            }
            
            // Draw text using SpriteBatch
            spriteBatch.DrawText(font, text, position, fontSize, color: color, layerDepth: 1.5f);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error rendering text '{text}': {ex.Message}");
        }
    }
    
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _cachedFont?.Dispose();
        _disposed = true;
    }
} 