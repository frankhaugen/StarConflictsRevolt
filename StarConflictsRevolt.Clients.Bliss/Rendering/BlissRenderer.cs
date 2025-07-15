using System.Numerics;
using StarConflictsRevolt.Clients.Bliss.Core;

namespace StarConflictsRevolt.Clients.Bliss.Rendering;

/// <summary>
/// Mock Bliss implementation of the 2D renderer interface.
/// This will be replaced with actual Bliss types when the library is properly integrated.
/// </summary>
public class BlissRenderer : IRenderer2D
{
    private Matrix3x2 _currentCameraMatrix = Matrix3x2.Identity;
    private bool _isDrawing = false;

    public void Begin(Matrix3x2 cameraMatrix)
    {
        _currentCameraMatrix = cameraMatrix;
        _isDrawing = true;
        // Mock: Begin sprite batch
    }

    public void Draw(ISprite sprite, Vector2 position, Color color = default)
    {
        if (!_isDrawing) return;
        
        // Mock: Draw sprite at position with color
        // In real implementation, this would use Bliss's sprite batch
    }

    public void DrawText(string text, Vector2 position, Color color, float scale = 1.0f)
    {
        if (!_isDrawing) return;
        
        // Mock: Draw text at position with color and scale
        // In real implementation, this would use Bliss's font rendering
    }

    public void End()
    {
        _isDrawing = false;
        // Mock: End sprite batch and flush to GPU
    }

    public void Clear(Color color)
    {
        // Mock: Clear screen with color
        // In real implementation, this would clear the Bliss window
    }
}

/// <summary>
/// Mock Bliss-specific sprite implementation.
/// </summary>
public class BlissSprite : ISprite
{
    public object Texture { get; } // Mock texture object

    public float Width { get; }
    public float Height { get; }

    public BlissSprite(float width, float height)
    {
        Width = width;
        Height = height;
        Texture = new object(); // Mock texture
    }
} 