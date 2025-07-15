using Bliss.CSharp;
using Bliss.CSharp.Graphics.Pipelines;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Contexts;
using Bliss.CSharp.Windowing;
using Veldrid;

namespace StarConflictsRevolt.Clients.Bliss;

public class RenderLoopService : BackgroundService
{
    private readonly ILogger<RenderLoopService> _logger;
    private readonly IWindow _window;
    private readonly GraphicsDevice _graphicsDevice;

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
        CommandList commandList = _graphicsDevice.ResourceFactory.CreateCommandList();

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

        while (!stoppingToken.IsCancellationRequested && _window.Exists)
        {
            _window.PumpEvents(); // Process window events.
            Input.Begin(); // Start input processing.

            if (!_window.Exists)
            {
                break; // Exit loop if the main window no longer exists.
            }
            
            var viewport = new Viewport(0f, 0f, _window.GetWidth(), _window.GetHeight(), 0.0f, 1.0f);
            var pipeline = new SimplePipeline(_graphicsDevice, new SimplePipelineDescription());
            
            // Simple render loop logic for demonstration purposes with a spinning cube.
            commandList.Begin();
            commandList.SetFramebuffer(_graphicsDevice.SwapchainFramebuffer);
            commandList.ClearColorTarget(0, RgbaFloat.BLACK);
            commandList.ClearDepthStencil(1.0f);
            commandList.SetViewport(0, viewport);
            commandList.SetScissorRect(0,0, 0, (uint)_window.GetWidth(), (uint)_window.GetHeight());
            commandList.SetPipeline(pipeline.Pipeline); // Assuming SimplePipeline is a predefined pipeline for rendering.
            commandList.Draw(36); // Draw a spinning cube (assuming 36 vertices).
            commandList.End();
            _graphicsDevice.SubmitCommands(commandList);
            _graphicsDevice.SwapBuffers(); // Swap buffers to display the rendered frame.
            Input.End(); // End input processing.
            await Task.Delay(16, stoppingToken); // Wait for ~16ms to maintain ~60 FPS.
        }
    }
}
