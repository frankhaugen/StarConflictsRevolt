using Bliss.CSharp;
using Bliss.CSharp.Graphics.Rendering.Batches.Primitives;
using Bliss.CSharp.Graphics.Rendering.Batches.Sprites;
using Bliss.CSharp.Graphics.Rendering.Renderers;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Contexts;
using Bliss.CSharp.Windowing;
using StarConflictsRevolt.Clients.Bliss.Core.UI;
using StarConflictsRevolt.Clients.Bliss.Core.UI.Interfaces;
using Veldrid;

namespace StarConflictsRevolt.Clients.Bliss;

/// <summary>
/// UI render loop service that integrates the new UI system with the existing render loop.
/// Follows the Adapter pattern by adapting the UI system to work with the existing rendering infrastructure.
/// </summary>
public class UIRenderLoopService
{
    private readonly IWindow _window;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly IScreenManager _screenManager;
    private readonly IInputHandler _inputHandler;
    private readonly UIScalingService _scalingService;
    private readonly ImmediateRenderer _immediateRenderer;
    private readonly PrimitiveBatch _primitiveBatch;
    private readonly SpriteBatch _spriteBatch;
    private readonly CommandList _commandList;
    private bool _isRunning = false;
    
    public UIRenderLoopService(
        IWindow window,
        GraphicsDevice graphicsDevice,
        IScreenManager screenManager,
        IInputHandler inputHandler,
        UIScalingService scalingService)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));
        _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
        _screenManager = screenManager ?? throw new ArgumentNullException(nameof(screenManager));
        _inputHandler = inputHandler ?? throw new ArgumentNullException(nameof(inputHandler));
        _scalingService = scalingService ?? throw new ArgumentNullException(nameof(scalingService));
        
        // Initialize Bliss global resources
        GlobalResource.Init(graphicsDevice);
        
        // Initialize input system
        if (_window is Sdl3Window)
        {
            Input.Init(new Sdl3InputContext(_window));
        }
        else
        {
            throw new Exception("This type of window is not supported by the InputContext!");
        }
        
        // Initialize rendering components
        _immediateRenderer = new ImmediateRenderer(graphicsDevice);
        _primitiveBatch = new PrimitiveBatch(graphicsDevice, window, 1000);
        _spriteBatch = new SpriteBatch(graphicsDevice, window, 1000);
        _commandList = graphicsDevice.ResourceFactory.CreateCommandList();
        
        // Set up event handlers
        _window.Closed += OnWindowClosed;
        _window.Resized += OnWindowResized;
    }
    
    public void Run()
    {
        _isRunning = true;
        
        while (_isRunning && _window.Exists)
        {
            _window.PumpEvents(); // Process window events
            Input.Begin(); // Start input processing
            
            // Update input handler
            _inputHandler.Update();
            
            // Handle input for current screen
            _screenManager.HandleInput();
            
            // Update current screen
            _screenManager.Update(1f / 60f); // Fixed 60 FPS for now
            
            // Render current screen
            Render();
            
            Input.End(); // End input processing
        }
    }
    
    private void Render()
    {
        // Begin frame
        _commandList.Begin();
        _commandList.SetFramebuffer(_graphicsDevice.SwapchainFramebuffer);
        _commandList.ClearColorTarget(0, new RgbaFloat(0, 0, 0, 1));
        
        // Render current screen
        _screenManager.Render(
            _immediateRenderer,
            _primitiveBatch,
            _spriteBatch,
            _commandList,
            _graphicsDevice.SwapchainFramebuffer);
        
        // End frame
        _commandList.End();
        _graphicsDevice.SubmitCommands(_commandList);
        _graphicsDevice.SwapBuffers();
    }
    
    private void OnWindowClosed()
    {
        _isRunning = false;
    }
    
    private void OnWindowResized()
    {
        // Update scaling service when window is resized
        _scalingService.UpdateResolution();
    }
    
    public void Dispose()
    {
        _commandList?.Dispose();
        _spriteBatch?.Dispose();
        _primitiveBatch?.Dispose();
        _immediateRenderer?.Dispose();
        GlobalResource.Destroy();
    }
} 