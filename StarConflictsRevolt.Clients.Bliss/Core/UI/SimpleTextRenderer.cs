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
            
            // Try to load Windows Arial font from system fonts directory
            try 
            { 
                _cachedFont = new Font("C:\\Windows\\Fonts\\arial.ttf"); 
                Console.WriteLine("Successfully loaded Arial font from Windows Fonts directory");
                return _cachedFont;
            } 
            catch (Exception ex) 
            { 
                Console.WriteLine($"Failed to load Arial from Windows Fonts: {ex.Message}");
            }
            
            // Try alternative system font paths
            try 
            { 
                _cachedFont = new Font("C:\\Windows\\Fonts\\calibri.ttf"); 
                Console.WriteLine("Successfully loaded Calibri font as fallback");
                return _cachedFont;
            } 
            catch (Exception ex) 
            { 
                Console.WriteLine($"Failed to load Calibri: {ex.Message}");
            }
            
            return null!;
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
                Console.WriteLine("Warning: Could not load any system font, using fallback");
                DrawTextCenteredFallback(text, bounds, null, fontName, fontSize, color);
                return;
            }
            
            // Calculate text position (centered)
            var spriteFont = font.GetSpriteFont(fontSize);
            var textSize = spriteFont.MeasureString(text);
            var textX = bounds.X + (bounds.Width - textSize.X) / 2;
            var textY = bounds.Y + (bounds.Height - textSize.Y) / 2;
            
            // Draw text using SpriteBatch
            spriteBatch.DrawText(font, text, new Vector2(textX, textY), fontSize, color: color);
        }
        catch (Exception ex)
        {
            // Fallback to simple rectangle representation
            DrawTextCenteredFallback(text, bounds, null, fontName, fontSize, color);
        }
    }
    
    /// <summary>
    /// Fallback text rendering using simple colored rectangles.
    /// This is a temporary solution while we debug the text rendering pipeline.
    /// </summary>
    public void DrawTextCenteredFallback(string text, RectangleF bounds, PrimitiveBatch primitiveBatch, string fontName, float fontSize, Color color)
    {
        
        // Draw a simple colored rectangle to represent the text
        var textRect = new RectangleF(
            bounds.X + bounds.Width * 0.1f,  // 10% margin from left
            bounds.Y + bounds.Height * 0.3f, // 30% margin from top
            bounds.Width * 0.8f,             // 80% of bounds width
            bounds.Height * 0.4f);           // 40% of bounds height
        
        // Use a bright color to make it visible
        var fallbackColor = new Color(255, 255, 255, 255); // White
        
        if (primitiveBatch != null)
        {
            primitiveBatch.DrawFilledRectangle(textRect, Vector2.Zero, 0f, 0.9f, fallbackColor);
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