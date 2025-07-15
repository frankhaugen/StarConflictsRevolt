using Bliss.CSharp.Graphics.Rendering.Batches.Primitives;
using Bliss.CSharp.Graphics.Rendering.Batches.Sprites;
using Bliss.CSharp.Graphics.Rendering.Renderers;
using Veldrid;

namespace StarConflictsRevolt.Clients.Bliss.Core.UI.Interfaces;

/// <summary>
/// Represents a UI screen that can be displayed and managed by the screen manager.
/// Follows the Single Responsibility Principle by focusing only on screen-specific logic.
/// </summary>
public interface IScreen
{
    /// <summary>
    /// Gets the unique identifier for this screen.
    /// </summary>
    string ScreenId { get; }
    
    /// <summary>
    /// Gets the display title for this screen.
    /// </summary>
    string Title { get; }
    
    /// <summary>
    /// Gets whether this screen is currently active and should receive input.
    /// </summary>
    bool IsActive { get; }
    
    /// <summary>
    /// Gets whether this screen is visible and should be rendered.
    /// </summary>
    bool IsVisible { get; }
    
    /// <summary>
    /// Called when the screen is activated (becomes the current screen).
    /// </summary>
    void OnActivate();
    
    /// <summary>
    /// Called when the screen is deactivated (no longer the current screen).
    /// </summary>
    void OnDeactivate();
    
    /// <summary>
    /// Updates the screen logic.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last update in seconds.</param>
    void Update(float deltaTime);
    
    /// <summary>
    /// Renders the screen.
    /// </summary>
    void Render(ImmediateRenderer immediateRenderer, 
                PrimitiveBatch primitiveBatch,
                SpriteBatch spriteBatch,
                CommandList commandList,
                Framebuffer framebuffer);
    
    /// <summary>
    /// Handles input for this screen.
    /// </summary>
    void HandleInput();
    
    /// <summary>
    /// Event fired when this screen requests navigation to another screen.
    /// </summary>
    event Action<string>? NavigationRequested;
    
    /// <summary>
    /// Event fired when this screen requests to exit the application.
    /// </summary>
    event Action? ExitRequested;
} 