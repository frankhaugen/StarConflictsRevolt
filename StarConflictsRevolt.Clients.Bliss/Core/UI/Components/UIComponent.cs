using Bliss.CSharp.Graphics.Rendering.Batches.Primitives;
using Bliss.CSharp.Graphics.Rendering.Batches.Sprites;
using Bliss.CSharp.Graphics.Rendering.Renderers;
using Veldrid;

namespace StarConflictsRevolt.Clients.Bliss.Core.UI.Components;

/// <summary>
/// Base interface for all UI components.
/// Follows the Component pattern for consistent UI component behavior.
/// </summary>
public interface UIComponent
{
    /// <summary>
    /// Updates the component logic.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last update in seconds.</param>
    void Update(float deltaTime);
    
    /// <summary>
    /// Renders the component.
    /// </summary>
    void Render(ImmediateRenderer immediateRenderer,
                PrimitiveBatch primitiveBatch,
                SpriteBatch spriteBatch,
                CommandList commandList,
                Framebuffer framebuffer);
    
    /// <summary>
    /// Sets whether this component is selected/focused.
    /// </summary>
    /// <param name="selected">Whether the component should be selected.</param>
    void SetSelected(bool selected);
    
    /// <summary>
    /// Gets whether this component is currently selected/focused.
    /// </summary>
    bool IsSelected { get; }
    
    /// <summary>
    /// Gets whether this component is enabled and can receive input.
    /// </summary>
    bool IsEnabled { get; set; }
} 