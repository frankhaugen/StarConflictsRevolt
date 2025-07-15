using Bliss.CSharp;
using Bliss.CSharp.Graphics.Rendering.Batches.Primitives;
using Bliss.CSharp.Graphics.Rendering.Batches.Sprites;
using Bliss.CSharp.Graphics.Rendering.Renderers;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Contexts;
using Bliss.CSharp.Windowing;
using Veldrid;
using Bliss.CSharp.Transformations;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Interact.Keyboards;
using StarConflictsRevolt.Clients.Bliss.Core;
using StarConflictsRevolt.Clients.Bliss.Core.UI;
using StarConflictsRevolt.Clients.Bliss.Views;

namespace StarConflictsRevolt.Clients.Bliss;

public class RenderLoopService : IDisposable
{
    private readonly ILogger<RenderLoopService> _logger;
    private readonly IWindow _window;
    private readonly GraphicsDevice _graphicsDevice;
    private CommandList _commandList = null!;
    private ImmediateRenderer _immediateRenderer = null!;
    private SpriteBatch _spriteBatch = null!;
    private PrimitiveBatch _primitiveBatch = null!;
    private ViewManager _viewManager = null!;
    private UIManager _uiManager = null!;
    private float _lastFrameTime = 0f;
    private bool _disposed = false;

    /// <inheritdoc />
    public RenderLoopService(ILogger<RenderLoopService> logger, IWindow window, GraphicsDevice graphicsDevice)
    {
        _logger = logger;
        _window = window;
        _graphicsDevice = graphicsDevice;
    }

    /// <summary>
    /// Runs the render loop on the main thread. This method blocks until the window is closed.
    /// </summary>
    public void Run()
    {
        _logger.LogInformation("Render loop started");

        // Initialize command list.
        _commandList = _graphicsDevice.ResourceFactory.CreateCommandList();

        // Initialize global resources.
        GlobalResource.Init(_graphicsDevice);

        // Initialize input.
        if (_window is Sdl3Window)
        {
            Input.Init(new Sdl3InputContext(_window));
        }
        else
        {
            throw new Exception("This type of window is not supported by the InputContext!");
        }

        // Initialize renderers
        _immediateRenderer = new ImmediateRenderer(_graphicsDevice);
        _spriteBatch = new SpriteBatch(_graphicsDevice, _window);
        _primitiveBatch = new PrimitiveBatch(_graphicsDevice, _window);

        // Initialize view manager and register views
        _viewManager = new ViewManager();
        
        var menuView = new MenuView();
        menuView.ViewRequested += (viewName) => _viewManager.SwitchToView(viewName);
        
        _viewManager.RegisterView(menuView);
        _viewManager.RegisterView(new GalaxyView());
        _viewManager.RegisterView(new TacticalBattleView());

        // Initialize UI manager
        _uiManager = new UIManager(_window.GetWidth(), _window.GetHeight());
        _uiManager.ViewRequested += (viewName) => _viewManager.SwitchToView(viewName);

        // Main render loop - runs on the main thread
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        while (_window.Exists && !_disposed)
        {
            _window.PumpEvents(); // Process window events.
            Input.Begin(); // Start input processing.

            if (!_window.Exists)
            {
                break; // Exit loop if the main window no longer exists.
            }
            
            // Calculate delta time
            var currentTime = stopwatch.ElapsedMilliseconds / 1000.0f;
            var deltaTime = currentTime - _lastFrameTime;
            _lastFrameTime = currentTime;
            
            // Handle input
            HandleInput();
            
            // Update current view
            _viewManager.CurrentView?.Update((float)deltaTime);
            
            // Update UI manager
            _uiManager.Update((float)deltaTime);
            
            // Render frame
            RenderFrame();
            
            Input.End(); // End input processing.
            
            // Small delay to maintain ~60 FPS
            Thread.Sleep(16);
        }
        
        _logger.LogInformation("Render loop ended");
    }
    
    private void HandleInput()
    {
        // Handle UI input first (modals, navigation bar)
        _uiManager.HandleInput();
        
        // If a modal is open, don't process other input
        if (_uiManager.ModalDialog.IsVisible)
        {
            return;
        }
        
        // Handle keyboard input for view switching
        if (Input.IsKeyPressed(KeyboardKey.F1))
        {
            _viewManager.SwitchToView("Galaxy Overview");
        }
        else if (Input.IsKeyPressed(KeyboardKey.F2))
        {
            _viewManager.SwitchToView("Tactical Battle");
        }
        else if (Input.IsKeyPressed(KeyboardKey.Escape))
        {
            _viewManager.SwitchToView("Main Menu");
        }
        
        // Handle menu navigation if current view is MenuView
        if (_viewManager.CurrentView is MenuView menuView)
        {
            if (Input.IsKeyPressed(KeyboardKey.Up))
            {
                menuView.SelectPrevious();
            }
            else if (Input.IsKeyPressed(KeyboardKey.Down))
            {
                menuView.SelectNext();
            }
            else if (Input.IsKeyPressed(KeyboardKey.Enter))
            {
                menuView.ActivateSelected();
            }
        }
        
        // Demo modal functionality
        if (Input.IsKeyPressed(KeyboardKey.M))
        {
            _uiManager.ShowMessage("Demo", "This is a demo modal dialog!", "OK");
        }
        else if (Input.IsKeyPressed(KeyboardKey.C))
        {
            _uiManager.ShowConfirmation("Confirm Action", "Are you sure you want to proceed?", 
                () => _uiManager.ShowMessage("Success", "Action confirmed!", "OK"));
        }
    }

    private void RenderFrame()
    {
        _commandList.Begin();
        _commandList.SetFramebuffer(_graphicsDevice.SwapchainFramebuffer);
        _commandList.ClearColorTarget(0, new RgbaFloat(0.05f, 0.05f, 0.1f, 1.0f)); // Dark space background
        // Only clear depth if the framebuffer has a depth target
        if (_graphicsDevice.SwapchainFramebuffer.OutputDescription.DepthAttachment != null)
        {
            _commandList.ClearDepthStencil(1.0f);
        }

        // Render the current view
        if (_viewManager.CurrentView != null)
        {
            _viewManager.CurrentView.Render(
                _immediateRenderer,
                _primitiveBatch,
                _spriteBatch,
                _commandList,
                _graphicsDevice.SwapchainFramebuffer
            );
        }
        
        // Render UI components
        _uiManager.Render(
            _immediateRenderer,
            _primitiveBatch,
            _spriteBatch,
            _commandList,
            _graphicsDevice.SwapchainFramebuffer
        );

        _commandList.End();
        _graphicsDevice.SubmitCommands(_commandList);
        _graphicsDevice.SwapBuffers(); // Swap buffers to display the rendered frame.
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _commandList?.Dispose();
            _immediateRenderer?.Dispose();
            _spriteBatch?.Dispose();
            _primitiveBatch?.Dispose();
        }
    }
}
