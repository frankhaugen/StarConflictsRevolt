using System.Numerics;
using Raylib_CSharp.Camera.Cam2D;

namespace StarConflictsRevolt.Clients.Raylib.Rendering.Core;

/// <summary>
/// Defines the game viewport area where the world is rendered, separate from UI overlays.
/// </summary>
public class GameViewport
{
    private readonly Rectangle _bounds;
    private Camera2D _camera;
    private Vector2 _cameraTarget;
    private float _cameraZoom = 1.0f;
    private float _cameraRotation = 0.0f;
    
    public Rectangle Bounds => _bounds;
    public Camera2D Camera => _camera;
    public Vector2 CameraTarget 
    { 
        get => _cameraTarget;
        set 
        {
            _cameraTarget = value;
            UpdateCamera();
        }
    }
    
    public float CameraZoom 
    { 
        get => _cameraZoom;
        set 
        {
            _cameraZoom = Math.Clamp(value, 0.1f, 10.0f);
            UpdateCamera();
        }
    }
    
    public float CameraRotation 
    { 
        get => _cameraRotation;
        set 
        {
            _cameraRotation = value;
            UpdateCamera();
        }
    }
    
    public GameViewport(Rectangle bounds)
    {
        _bounds = bounds;
        _camera = new Camera2D
        {
            Target = Vector2.Zero,
            Offset = new Vector2(bounds.Width / 2f, bounds.Height / 2f),
            Rotation = 0.0f,
            Zoom = 1.0f
        };
        _cameraTarget = Vector2.Zero;
    }
    
    public GameViewport(int x, int y, int width, int height) : this(new Rectangle(x, y, width, height))
    {
    }
    
    private void UpdateCamera()
    {
        _camera.Target = _cameraTarget;
        _camera.Zoom = _cameraZoom;
        _camera.Rotation = _cameraRotation;
    }
    
    /// <summary>
    /// Converts screen coordinates to world coordinates within the viewport.
    /// </summary>
    public Vector2 ScreenToWorld(Vector2 screenPosition)
    {
        // Adjust for viewport offset
        var viewportPos = screenPosition - new Vector2(_bounds.X, _bounds.Y);
        
        // Convert using camera
        var worldPos = viewportPos - _camera.Offset;
        worldPos = worldPos / _camera.Zoom;
        worldPos = worldPos + _camera.Target;
        
        return worldPos;
    }
    
    /// <summary>
    /// Converts world coordinates to screen coordinates within the viewport.
    /// </summary>
    public Vector2 WorldToScreen(Vector2 worldPosition)
    {
        // Convert using camera
        var screenPos = worldPosition - _camera.Target;
        screenPos = screenPos * _camera.Zoom;
        screenPos = screenPos + _camera.Offset;
        
        // Adjust for viewport offset
        screenPos += new Vector2(_bounds.X, _bounds.Y);
        
        return screenPos;
    }
    
    /// <summary>
    /// Checks if a screen position is within the viewport bounds.
    /// </summary>
    public bool IsInViewport(Vector2 screenPosition)
    {
        return screenPosition.X >= _bounds.X && 
               screenPosition.X <= _bounds.X + _bounds.Width &&
               screenPosition.Y >= _bounds.Y && 
               screenPosition.Y <= _bounds.Y + _bounds.Height;
    }
    
    /// <summary>
    /// Gets the center of the viewport in screen coordinates.
    /// </summary>
    public Vector2 GetCenter()
    {
        return new Vector2(_bounds.X + _bounds.Width / 2f, _bounds.Y + _bounds.Height / 2f);
    }
    
    /// <summary>
    /// Resizes the viewport to new bounds.
    /// </summary>
    public void Resize(Rectangle newBounds)
    {
        var oldCenter = GetCenter();
        var newCenter = new Vector2(newBounds.X + newBounds.Width / 2f, newBounds.Y + newBounds.Height / 2f);
        
        // Update camera offset for new size
        _camera.Offset = new Vector2(newBounds.Width / 2f, newBounds.Height / 2f);
        
        // Adjust camera target to maintain visual center
        var offset = newCenter - oldCenter;
        _cameraTarget += offset / _camera.Zoom;
        
        UpdateCamera();
    }
} 