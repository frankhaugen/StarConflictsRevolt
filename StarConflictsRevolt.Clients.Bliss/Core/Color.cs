namespace StarConflictsRevolt.Clients.Bliss.Core;

public struct Color
{
    public byte R { get; }
    public byte G { get; }
    public byte B { get; }
    public byte A { get; }

    public Color(byte r, byte g, byte b, byte a = 255)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public static Color White => new Color(255, 255, 255);
    public static Color Black => new Color(0, 0, 0);
    public static Color Yellow => new Color(255, 255, 0);
    public static Color Green => new Color(0, 255, 0);
    public static Color Orange => new Color(255, 165, 0);
    public static Color Cyan => new Color(0, 255, 255);
    public static Color Gray => new Color(128, 128, 128);
} 