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

namespace StarConflictsRevolt.Clients.Bliss;

public class RenderLoopService : BackgroundService
{
    private readonly ILogger<RenderLoopService> _logger;
    private readonly IWindow _window;
    private readonly GraphicsDevice _graphicsDevice;
    private CommandList _commandList = null!;
    private ImmediateRenderer _immediateRenderer = null!;
    private SpriteBatch _spriteBatch = null!;
    private PrimitiveBatch _primitiveBatch = null!;
    private float _rotation = 0f;

    /// <inheritdoc />
    public RenderLoopService(ILogger<RenderLoopService> logger, IWindow window, GraphicsDevice graphicsDevice)
    {
        _logger = logger;
        _window = window;
        _graphicsDevice = graphicsDevice;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
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

        while (!stoppingToken.IsCancellationRequested && _window.Exists)
        {
            _window.PumpEvents(); // Process window events.
            Input.Begin(); // Start input processing.

            if (!_window.Exists)
            {
                break; // Exit loop if the main window no longer exists.
            }
            
            // Update rotation for animation
            _rotation += 1f;
            
            // Render frame
            RenderFrame();
            
            Input.End(); // End input processing.
            await Task.Delay(16, stoppingToken); // Wait for ~16ms to maintain ~60 FPS.
        }
    }

    private void RenderFrame()
    {
        _commandList.Begin();
        _commandList.SetFramebuffer(_graphicsDevice.SwapchainFramebuffer);
        _commandList.ClearColorTarget(0, new RgbaFloat(0.1f, 0.1f, 0.2f, 1.0f)); // Dark blue background
        // Only clear depth if the framebuffer has a depth target
        if (_graphicsDevice.SwapchainFramebuffer.OutputDescription.DepthAttachment != null)
        {
            _commandList.ClearDepthStencil(1.0f);
        }

        // Draw some 3D objects using ImmediateRenderer
        _immediateRenderer.DrawCube(_commandList, _graphicsDevice.SwapchainFramebuffer.OutputDescription, 
            new Transform { Translation = new System.Numerics.Vector3(0, 0, 0) }, 
            new System.Numerics.Vector3(1, 1, 1));
        
        _immediateRenderer.DrawSphere(_commandList, _graphicsDevice.SwapchainFramebuffer.OutputDescription, 
            new Transform { Translation = new System.Numerics.Vector3(3, 0, 0) }, 
            1, 20, 20);

        // Draw 2D primitives using PrimitiveBatch
        _primitiveBatch.Begin(_commandList, _graphicsDevice.SwapchainFramebuffer.OutputDescription);
        
        // Draw a rotating rectangle
        _primitiveBatch.DrawFilledRectangle(
            new global::Bliss.CSharp.Transformations.RectangleF(100, 100, 100, 100), 
            origin: new System.Numerics.Vector2(50, 50), 
            rotation: _rotation, 
            color: global::Bliss.CSharp.Colors.Color.Green);
        
        // Draw a circle
        _primitiveBatch.DrawFilledCircle(
            new System.Numerics.Vector2(300, 200), 
            50, 32, 0.5f, 
            global::Bliss.CSharp.Colors.Color.Red);
        
        _primitiveBatch.End();

        // Draw text using SpriteBatch
        _spriteBatch.Begin(_commandList, _graphicsDevice.SwapchainFramebuffer.OutputDescription);
        
        // Note: We would need a font to draw text, but for now just draw without text
        // _spriteBatch.DrawText(font, "Hello Bliss!", new System.Numerics.Vector2(10, 10), 24);
        
        _spriteBatch.End();

        _commandList.End();
        _graphicsDevice.SubmitCommands(_commandList);
        _graphicsDevice.SwapBuffers(); // Swap buffers to display the rendered frame.
    }
}
