using Veldrid;
using Bliss.CSharp.Fonts;
using Bliss.CSharp.Graphics.Rendering.Batches.Primitives;
using Bliss.CSharp.Graphics.Rendering.Batches.Sprites;
using Bliss.CSharp.Graphics.Rendering.Renderers;
using Bliss.CSharp.Windowing;

namespace StarConflictsRevolt.Clients.Bliss.Core.UI;

/// <summary>
/// Centralized resource manager for all Veldrid resources.
/// Follows the Resource Pool pattern to prevent disposal issues.
/// </summary>
public class ResourceManager : IDisposable
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly IWindow _window;
    private readonly Dictionary<string, Font> _fonts = new();
    private readonly List<IDisposable> _managedResources = new();
    private bool _disposed = false;
    
    // Core rendering resources
    public ImmediateRenderer ImmediateRenderer { get; private set; } = null!;
    public PrimitiveBatch PrimitiveBatch { get; private set; } = null!;
    public SpriteBatch SpriteBatch { get; private set; } = null!;
    public CommandList CommandList { get; private set; } = null!;
    
    public ResourceManager(GraphicsDevice graphicsDevice, IWindow window)
    {
        _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
        _window = window ?? throw new ArgumentNullException(nameof(window));
        // Don't initialize resources during construction - wait until first use
    }
    
    /// <summary>
    /// Initializes resources when first needed.
    /// </summary>
    public void Initialize()
    {
        if (_managedResources.Count == 0)
        {
            InitializeResources();
        }
    }
    
    private void InitializeResources()
    {
        // Initialize core rendering resources
        ImmediateRenderer = new ImmediateRenderer(_graphicsDevice);
        PrimitiveBatch = new PrimitiveBatch(_graphicsDevice, _window, 1000);
        SpriteBatch = new SpriteBatch(_graphicsDevice, _window, 1000);
        CommandList = _graphicsDevice.ResourceFactory.CreateCommandList();
        
        // Add to managed resources for disposal
        _managedResources.Add(ImmediateRenderer);
        _managedResources.Add(PrimitiveBatch);
        _managedResources.Add(SpriteBatch);
        _managedResources.Add(CommandList);
        
        // Load fonts
        LoadFonts();
    }
    
    private void LoadFonts()
    {
        try
        {
            var galaxyFont = new Font("Assets/Fonts/Galaxy.ttf");
            _fonts["Galaxy"] = galaxyFont;
            _fonts["Default"] = galaxyFont; // Ensure 'Default' always resolves
            _managedResources.Add(galaxyFont);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not load fonts: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Gets a font by name.
    /// </summary>
    public Font? GetFont(string fontName)
    {
        return _fonts.TryGetValue(fontName, out var font) ? font : null;
    }
    
    /// <summary>
    /// Begins a new frame with proper resource setup.
    /// </summary>
    public void BeginFrame(Framebuffer framebuffer)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(ResourceManager));
        
        CommandList.Begin();
        CommandList.SetFramebuffer(framebuffer);
        CommandList.ClearColorTarget(0, new RgbaFloat(0, 0, 0, 1));
        
        // Begin the primitive batch for UI elements
        PrimitiveBatch.Begin(CommandList, framebuffer.OutputDescription);
        
        // Begin the sprite batch for text rendering
        SpriteBatch.Begin(CommandList, framebuffer.OutputDescription);
    }
    
    /// <summary>
    /// Ends the current frame and submits commands.
    /// </summary>
    public void EndFrame()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(ResourceManager));
        
        // End the sprite batch
        SpriteBatch.End();
        
        // End the primitive batch
        PrimitiveBatch.End();
        
        CommandList.End();
        _graphicsDevice.SubmitCommands(CommandList);
        _graphicsDevice.SwapBuffers();
    }
    
    /// <summary>
    /// Recreates all resources (useful after window resize).
    /// </summary>
    public void RecreateResources()
    {
        if (_disposed) return;
        
        try
        {
            // Dispose existing resources safely
            foreach (var resource in _managedResources)
            {
                try
                {
                    resource?.Dispose();
                }
                catch (Exception ex)
                {
                    // Log but don't crash - resource might already be disposed
                    Console.WriteLine($"Warning: Error disposing resource: {ex.Message}");
                }
            }
            _managedResources.Clear();
            _fonts.Clear();
            
            // Reset properties to null
            ImmediateRenderer = null!;
            PrimitiveBatch = null!;
            SpriteBatch = null!;
            CommandList = null!;
            
            // Recreate resources
            InitializeResources();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error recreating resources: {ex.Message}");
            // Don't rethrow - try to continue with existing resources
        }
    }
    
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            
            foreach (var resource in _managedResources)
            {
                resource?.Dispose();
            }
            _managedResources.Clear();
            _fonts.Clear();
        }
    }
} 