using System.Numerics;

namespace StarConflictsRevolt.Clients.Bliss.Core;

/// <summary>
/// Thin Bliss adapter for 2D rendering operations.
/// </summary>
public interface IRenderer2D
{
    /// <summary>
    /// Begins a rendering batch with the specified camera transformation matrix.
    /// </summary>
    /// <param name="cameraMatrix">The camera transformation matrix</param>
    void Begin(Matrix3x2 cameraMatrix);
    
    /// <summary>
    /// Draws a sprite at the specified position.
    /// </summary>
    /// <param name="sprite">The sprite to draw</param>
    /// <param name="position">The position to draw at</param>
    /// <param name="color">The color tint to apply</param>
    void Draw(ISprite sprite, Vector2 position, Color color = default);
    
    /// <summary>
    /// Draws text at the specified position.
    /// </summary>
    /// <param name="text">The text to draw</param>
    /// <param name="position">The position to draw at</param>
    /// <param name="color">The color of the text</param>
    /// <param name="scale">The scale of the text</param>
    void DrawText(string text, Vector2 position, Color color, float scale = 1.0f);
    
    /// <summary>
    /// Ends the current rendering batch and flushes to GPU.
    /// </summary>
    void End();
    
    /// <summary>
    /// Clears the screen with the specified color.
    /// </summary>
    /// <param name="color">The color to clear with</param>
    void Clear(Color color);
} 