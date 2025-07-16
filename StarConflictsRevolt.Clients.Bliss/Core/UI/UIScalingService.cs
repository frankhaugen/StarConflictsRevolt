using Bliss.CSharp.Windowing;
using RectangleF = Bliss.CSharp.Transformations.RectangleF;

namespace StarConflictsRevolt.Clients.Bliss.Core.UI;

/// <summary>
/// Service for handling UI scaling and coordinate transformation.
/// Converts between screen coordinates and normalized UI coordinates.
/// </summary>
public class UIScalingService
{
    private readonly IWindow _window;
    private Vector2 _baseResolution = new(1920, 1080); // Base resolution for UI design
    private Vector2 _currentResolution;
    private Vector2 _scaleFactor;
    private Matrix4x4 _transformMatrix;
    
    public Vector2 ScaleFactor => _scaleFactor;
    public Vector2 CurrentResolution => _currentResolution;
    public Vector2 BaseResolution => _baseResolution;
    
    public UIScalingService(IWindow window)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));
        UpdateResolution();
    }
    
    /// <summary>
    /// Updates the resolution and recalculates scaling factors.
    /// Should be called when the window is resized.
    /// </summary>
    public void UpdateResolution()
    {
        _currentResolution = new Vector2(_window.GetWidth(), _window.GetHeight());
        _scaleFactor = _currentResolution / _baseResolution;
        
        // Create transform matrix for scaling
        _transformMatrix = Matrix4x4.CreateScale(_scaleFactor.X, _scaleFactor.Y, 1.0f);
    }
    
    /// <summary>
    /// Converts a position from base resolution (1920x1080) to current window resolution.
    /// </summary>
    /// <param name="basePosition">Position in base resolution coordinates</param>
    /// <returns>Position scaled to current window resolution</returns>
    public Vector2 ScalePosition(Vector2 basePosition)
    {
        return Vector2.Transform(basePosition, _transformMatrix);
    }
    
    /// <summary>
    /// Converts a size from base resolution to current window resolution.
    /// </summary>
    /// <param name="baseSize">Size in base resolution coordinates</param>
    /// <returns>Size scaled to current window resolution</returns>
    public Vector2 ScaleSize(Vector2 baseSize)
    {
        return baseSize * _scaleFactor;
    }
    
    /// <summary>
    /// Converts a rectangle from base resolution to current window resolution.
    /// </summary>
    /// <param name="baseRect">Rectangle in base resolution coordinates</param>
    /// <returns>Rectangle scaled to current window resolution</returns>
    public RectangleF ScaleRectangle(RectangleF baseRect)
    {
        var scaledPosition = ScalePosition(new Vector2(baseRect.X, baseRect.Y));
        var scaledSize = ScaleSize(new Vector2(baseRect.Width, baseRect.Height));
        
        return new RectangleF(scaledPosition.X, scaledPosition.Y, scaledSize.X, scaledSize.Y);
    }
    
    /// <summary>
    /// Scales a font size from base resolution to current window resolution.
    /// </summary>
    /// <param name="baseFontSize">Font size for base resolution</param>
    /// <returns>Font size scaled to current window resolution</returns>
    public float ScaleFontSize(float baseFontSize)
    {
        return baseFontSize * Math.Min(_scaleFactor.X, _scaleFactor.Y);
    }
    
    /// <summary>
    /// Centers a UI element horizontally on the screen.
    /// </summary>
    /// <param name="elementWidth">Width of the element in base resolution</param>
    /// <returns>X coordinate for centered positioning</returns>
    public float CenterHorizontally(float elementWidth)
    {
        return (_baseResolution.X - elementWidth) / 2f;
    }
    
    /// <summary>
    /// Centers a UI element vertically on the screen.
    /// </summary>
    /// <param name="elementHeight">Height of the element in base resolution</param>
    /// <returns>Y coordinate for centered positioning</returns>
    public float CenterVertically(float elementHeight)
    {
        return (_baseResolution.Y - elementHeight) / 2f;
    }
} 