using Bliss.CSharp.Graphics.Rendering.Batches.Primitives;
using Bliss.CSharp.Graphics.Rendering.Batches.Sprites;
using Bliss.CSharp.Graphics.Rendering.Renderers;
using Veldrid;

namespace StarConflictsRevolt.Clients.Bliss.Core.UI.Interfaces;

/// <summary>
/// Manages screen transitions and navigation in the application.
/// Follows the Open/Closed Principle by allowing new screens to be added without modifying existing code.
/// </summary>
public interface IScreenManager
{
    /// <summary>
    /// Gets the currently active screen.
    /// </summary>
    IScreen? CurrentScreen { get; }
    
    /// <summary>
    /// Gets the previous screen in the navigation stack.
    /// </summary>
    IScreen? PreviousScreen { get; }
    
    /// <summary>
    /// Registers a screen with the manager.
    /// </summary>
    /// <param name="screen">The screen to register.</param>
    void RegisterScreen(IScreen screen);
    
    /// <summary>
    /// Navigates to a screen by its ID.
    /// </summary>
    /// <param name="screenId">The ID of the screen to navigate to.</param>
    /// <returns>True if navigation was successful, false otherwise.</returns>
    bool NavigateTo(string screenId);
    
    /// <summary>
    /// Navigates back to the previous screen.
    /// </summary>
    /// <returns>True if navigation was successful, false if there's no previous screen.</returns>
    bool NavigateBack();
    
    /// <summary>
    /// Gets a screen by its ID.
    /// </summary>
    /// <param name="screenId">The ID of the screen to retrieve.</param>
    /// <returns>The screen if found, null otherwise.</returns>
    IScreen? GetScreen(string screenId);
    
    /// <summary>
    /// Gets all registered screens.
    /// </summary>
    /// <returns>An enumerable of all registered screens.</returns>
    IEnumerable<IScreen> GetAllScreens();
    
    /// <summary>
    /// Updates the current screen.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last update in seconds.</param>
    void Update(float deltaTime);
    
    /// <summary>
    /// Renders the current screen.
    /// </summary>
    void Render(ImmediateRenderer immediateRenderer,
                PrimitiveBatch primitiveBatch,
                SpriteBatch spriteBatch,
                CommandList commandList,
                Framebuffer framebuffer);
    
    /// <summary>
    /// Handles input for the current screen.
    /// </summary>
    void HandleInput();
} 