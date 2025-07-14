using System.Numerics;

namespace StarConflictsRevolt.Clients.Raylib.Rendering.Core;

/// <summary>
/// Rectangle representation for bounds checking.
/// </summary>
public struct Rectangle
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    
    public Rectangle(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
    
    public bool Contains(Vector2 point)
    {
        return point.X >= X && point.X <= X + Width && 
               point.Y >= Y && point.Y <= Y + Height;
    }
    
    public static Rectangle FromPositionAndSize(Vector2 position, Vector2 size)
    {
        return new Rectangle(position.X, position.Y, size.X, size.Y);
    }
}