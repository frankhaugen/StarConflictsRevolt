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
    private readonly ResourceManager _resourceManager;
    private bool _disposed = false;

    public SimpleTextRenderer(ResourceManager resourceManager)
    {
        _resourceManager = resourceManager ?? throw new ArgumentNullException(nameof(resourceManager));
    }

    private Font? GetFont(string fontName)
    {
        // Try requested font, then 'Default', then 'Galaxy'
        return _resourceManager.GetFont(fontName)
            ?? _resourceManager.GetFont("Default")
            ?? _resourceManager.GetFont("Galaxy");
    }

    public void DrawTextCentered(string text, RectangleF bounds, SpriteBatch spriteBatch, string fontName, float fontSize, Color color, PrimitiveBatch primitiveBatch)
    {
        var font = GetFont(fontName);
        if (font == null)
        {
            // Draw a red rectangle to indicate missing font
            primitiveBatch.DrawFilledRectangle(bounds, color: new Color(255,0,0,128));
            return;
        }
        var spriteFont = font.GetSpriteFont(fontSize);
        var textSize = spriteFont.MeasureString(text);
        var textX = bounds.X + (bounds.Width - textSize.X) / 2;
        var textY = bounds.Y + (bounds.Height - textSize.Y) / 2;
        spriteBatch.DrawText(font, text, new Vector2(textX, textY), fontSize, color: color, layerDepth: 1.5f);
    }

    public void DrawText(string text, Vector2 position, SpriteBatch spriteBatch, string fontName, float fontSize, Color color, PrimitiveBatch primitiveBatch)
    {
        var font = GetFont(fontName);
        if (font == null)
        {
            // Draw a small red rectangle at the position to indicate missing font
            var errorRect = new RectangleF(position.X, position.Y, 40, 20);
            primitiveBatch.DrawFilledRectangle(errorRect, color: new Color(255,0,0,128));
            return;
        }
        spriteBatch.DrawText(font, text, position, fontSize, color: color, layerDepth: 1.5f);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
    }
} 