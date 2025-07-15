using System.Numerics;
using Bliss.CSharp.Graphics.Rendering.Renderers;
using Bliss.CSharp.Graphics.Rendering.Batches.Primitives;
using Bliss.CSharp.Graphics.Rendering.Batches.Sprites;
using Veldrid;

namespace StarConflictsRevolt.Clients.Bliss.Core;

/// <summary>
/// Marker interface implemented by every view/page in the StarConflictsRevolt client.
/// </summary>
public interface IGameView
{
    /// <summary>User‑visible title (localised in the UI layer).</summary>
    string Title { get; }
    
    /// <summary>Whether this view is currently active and should be rendered.</summary>
    bool IsActive { get; set; }
    
    /// <summary>Update the view logic.</summary>
    void Update(float deltaTime);
    
    /// <summary>Render the view.</summary>
    void Render(ImmediateRenderer immediateRenderer, 
                PrimitiveBatch primitiveBatch,
                SpriteBatch spriteBatch,
                CommandList commandList,
                Framebuffer framebuffer);
}

/// <summary>
/// Convenience abstract class that stores a <see cref="Title"/>. All concrete view‑models inherit from it.
/// </summary>
public abstract class GameView : IGameView
{
    public string Title { get; }
    public bool IsActive { get; set; } = true;
    
    protected GameView(string title)
    {
        Title = title;
    }
    
    public virtual void Update(float deltaTime) { }
    
    public abstract void Render(ImmediateRenderer immediateRenderer, 
                               PrimitiveBatch primitiveBatch,
                               SpriteBatch spriteBatch,
                               CommandList commandList,
                               Framebuffer framebuffer);
} 