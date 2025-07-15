using System.Numerics;

namespace StarConflictsRevolt.Clients.Bliss.Core;

/// <summary>
/// Represents a sprite that can be rendered by the Bliss renderer.
/// </summary>
public interface ISprite
{
    /// <summary>
    /// The width of the sprite in pixels.
    /// </summary>
    float Width { get; }
    
    /// <summary>
    /// The height of the sprite in pixels.
    /// </summary>
    float Height { get; }
    
    /// <summary>
    /// The size of the sprite as a vector.
    /// </summary>
    Vector2 Size => new(Width, Height);
} 