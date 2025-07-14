using System.Numerics;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Camera.Cam2D;
using Raylib_CSharp.Interact;

namespace StarConflictsRevolt.Clients.Raylib.Rendering.Core;

/// <summary>
/// Manages the UI system with high resolution support and modern sci-fi design.
/// </summary>
public class UIManager
{
    private readonly IUIRenderer _renderer;
    private readonly IInputState _inputState;
    private readonly Dictionary<string, IView> _views;
    private IView? _currentView;
    private Vector2? _lastPanMousePos = null;
    
    // High resolution support
    public float DpiScale { get; private set; } = 1.0f;
    public int BaseWidth { get; private set; } = 1920;
    public int BaseHeight { get; private set; } = 1080;
    
    // Enhanced camera system
    private Camera2D _camera;
    public Vector2 CameraTarget { get; set; }
    public float CameraZoom { get; set; } = 1.0f;
    public float CameraRotation { get; set; } = 0.0f;
    
    public Camera2D Camera => _camera;
    
    // Modern sci-fi color scheme (EVE Online inspired)
    public static class Colors
    {
        public static Color Primary = new Color(52, 152, 219, 255);      // Blue
        public static Color Secondary = new Color(155, 89, 182, 255);    // Purple
        public static Color Accent = new Color(241, 196, 15, 255);       // Yellow
        public static Color Success = new Color(46, 204, 113, 255);      // Green
        public static Color Warning = new Color(230, 126, 34, 255);      // Orange
        public static Color Danger = new Color(231, 76, 60, 255);        // Red
        public static Color Background = new Color(26, 26, 26, 255);     // Dark gray
        public static Color Surface = new Color(52, 73, 94, 255);        // Medium gray
        public static Color Text = new Color(236, 240, 241, 255);        // Light gray
        public static Color Border = new Color(149, 165, 166, 255);      // Border gray
    }
    
    public UIManager(IUIRenderer renderer, IInputState inputState)
    {
        _renderer = renderer;
        _inputState = inputState;
        _views = new Dictionary<string, IView>();
        
        InitializeCamera();
        DetectHighResolution();
    }
    
    private void InitializeCamera()
    {
        _camera = new Camera2D
        {
            Target = new Vector2(0, 0), // Center world at (0,0)
            Offset = new Vector2(BaseWidth / 2f, BaseHeight / 2f),
            Rotation = 0.0f,
            Zoom = 1.0f
        };
        CameraTarget = _camera.Target;
    }
    
    private void DetectHighResolution()
    {
        var screenWidth = _renderer.ScreenWidth;
        var screenHeight = _renderer.ScreenHeight;
        
        // Calculate DPI scale based on screen resolution
        if (screenWidth >= 3840 && screenHeight >= 2160) // 4K
        {
            DpiScale = 2.0f;
        }
        else if (screenWidth >= 2560 && screenHeight >= 1440) // 2K
        {
            DpiScale = 1.5f;
        }
        else if (screenWidth >= 1920 && screenHeight >= 1080) // 1080p
        {
            DpiScale = 1.0f;
        }
        else // Lower resolutions
        {
            DpiScale = 0.75f;
        }
        
        // Update base dimensions
        BaseWidth = (int)(screenWidth / DpiScale);
        BaseHeight = (int)(screenHeight / DpiScale);
    }
    
    public void RegisterView(IView view)
    {
        _views[view.ViewType.ToString()] = view;
    }
    
    public void SetCurrentView(GameView viewType)
    {
        var viewKey = viewType.ToString();
        if (_views.TryGetValue(viewKey, out var view))
        {
            _currentView = view;
        }
    }
    
    public void Update(float deltaTime)
    {
        // Update camera with smooth movement
        UpdateCamera(deltaTime);
    }
    
    public void Render()
    {
        // Set up camera for rendering
        _renderer.BeginMode2D();
        
        // Render current view
        if (_currentView != null)
        {
            _currentView.Draw();
        }
        
        _renderer.EndMode2D();
    }
    
    private void UpdateCamera(float deltaTime)
    {
        // Handle camera zoom with mouse wheel (centered on screen center)
        var wheelMove = _inputState.MouseWheelMove;
        if (Math.Abs(wheelMove) > 0.1f)
        {
            var mouseScreen = _inputState.MousePosition;
            var mouseWorldBefore = ScreenToWorld(mouseScreen);
            CameraZoom = Math.Clamp(CameraZoom + wheelMove * 0.1f, 0.1f, 10.0f);
            var mouseWorldAfter = ScreenToWorld(mouseScreen);
            CameraTarget += mouseWorldBefore - mouseWorldAfter;
        }

        // Pan with right mouse button
        if (_inputState.IsMouseButtonDown(MouseButton.Right))
        {
            if (_lastPanMousePos == null)
                _lastPanMousePos = _inputState.MousePosition;
            else
            {
                var mouseDelta = _inputState.MousePosition - _lastPanMousePos.Value;
                CameraTarget -= mouseDelta / CameraZoom; // Move camera target by delta in world space
                _lastPanMousePos = _inputState.MousePosition;
            }
        }
        else
        {
            _lastPanMousePos = null;
        }

        // Update camera properties
        _camera.Target = CameraTarget;
        _camera.Zoom = CameraZoom;
        _camera.Rotation = CameraRotation;
    }
    
    public Vector2 ScreenToWorld(Vector2 screenPosition)
    {
        // Convert screen coordinates to world coordinates using camera
        var worldPos = screenPosition - _camera.Offset;
        worldPos = worldPos / _camera.Zoom;
        worldPos = worldPos + _camera.Target;
        return worldPos;
    }
    
    public Vector2 WorldToScreen(Vector2 worldPosition)
    {
        // Convert world coordinates to screen coordinates using camera
        var screenPos = worldPosition - _camera.Target;
        screenPos = screenPos * _camera.Zoom;
        screenPos = screenPos + _camera.Offset;
        return screenPos;
    }
} 