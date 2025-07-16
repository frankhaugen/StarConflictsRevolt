using Bliss.CSharp;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Contexts;
using Bliss.CSharp.Windowing;
using StarConflictsRevolt.Clients.Bliss.Core.UI.Interfaces;
using Veldrid;

namespace StarConflictsRevolt.Clients.Bliss.Core.UI;

/// <summary>
/// Simplified render loop that uses the ResourceManager.
/// Follows the Template Method pattern for clean separation of concerns.
/// </summary>
public class SimpleRenderLoop : IDisposable
{
    private readonly IWindow _window;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly IScreenManager _screenManager;
    private readonly IInputHandler _inputHandler;
    private readonly UIScalingService _scalingService;
    private readonly ResourceManager _resourceManager;
    private readonly SimpleTextRenderer _textRenderer;
    private bool _isRunning = false;
    private bool _disposed = false;
    
    public SimpleRenderLoop(
        IWindow window,
        GraphicsDevice graphicsDevice,
        IScreenManager screenManager,
        IInputHandler inputHandler,
        UIScalingService scalingService,
        ResourceManager resourceManager,
        SimpleTextRenderer textRenderer)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));
        _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
        _screenManager = screenManager ?? throw new ArgumentNullException(nameof(screenManager));
        _inputHandler = inputHandler ?? throw new ArgumentNullException(nameof(inputHandler));
        _scalingService = scalingService ?? throw new ArgumentNullException(nameof(scalingService));
        _resourceManager = resourceManager ?? throw new ArgumentNullException(nameof(resourceManager));
        _textRenderer = textRenderer ?? throw new ArgumentNullException(nameof(textRenderer));
        
        // Initialize Bliss systems
        InitializeBlissSystems();
        
        // Set up event handlers
        _window.Closed += OnWindowClosed;
        _window.Resized += OnWindowResized;
    }
    
    private void InitializeBlissSystems()
    {
        // Initialize Bliss global resources
        GlobalResource.Init(_graphicsDevice);
        
        // Initialize input system
        if (_window is Sdl3Window)
        {
            Input.Init(new Sdl3InputContext(_window));
        }
        else
        {
            throw new Exception("This type of window is not supported by the InputContext!");
        }
    }
    
    public void Run()
    {
        Console.WriteLine("Starting simplified render loop...");
        
        // Initialize resources before starting render loop
        _resourceManager.Initialize();
        
        _isRunning = true;
        
        while (_isRunning && _window.Exists)
        {
            ProcessInput();
            Update();
            Render();
        }
        
        Console.WriteLine("Render loop ended.");
    }
    
    private void ProcessInput()
    {
        _window.PumpEvents();
        Input.Begin();
        
        _inputHandler.Update();
        _screenManager.HandleInput();
        
        Input.End();
    }
    
    private void Update()
    {
        _screenManager.Update(1f / 60f); // Fixed 60 FPS
    }
    
    private void Render()
    {
        var framebuffer = _graphicsDevice.SwapchainFramebuffer;
        
        // Begin frame
        _resourceManager.BeginFrame(framebuffer);
        
        // Render current screen
        _screenManager.Render(
            _resourceManager.ImmediateRenderer,
            _resourceManager.PrimitiveBatch,
            _resourceManager.SpriteBatch,
            _resourceManager.CommandList,
            framebuffer);
        
        // End frame
        _resourceManager.EndFrame();
    }
    
    private void OnWindowClosed()
    {
        _isRunning = false;
    }
    
    private void OnWindowResized()
    {
        // Update scaling service
        _scalingService.UpdateResolution();
        
        // Note: Resource recreation is not needed for simple UI scaling
        // The scaling service handles coordinate transformation
    }
    
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _resourceManager?.Dispose();
        }
    }
} 