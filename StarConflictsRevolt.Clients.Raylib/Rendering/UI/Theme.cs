using Raylib_CSharp.Colors;

namespace StarConflictsRevolt.Clients.Raylib.Rendering.UI;

public static class Theme
{
    // Color palette
    public static readonly Color Background = new(18, 22, 34, 230);
    public static readonly Color Panel = new(28, 32, 48, 220);
    public static readonly Color Accent = new(255, 140, 0, 255);
    public static readonly Color Border = new(100, 100, 120, 255);
    public static readonly Color Text = new(220, 220, 240, 255);
    public static readonly Color TextSecondary = new(180, 180, 200, 255);
    public static readonly Color Success = new(0, 180, 80, 255);
    public static readonly Color Warning = new(255, 180, 0, 255);
    public static readonly Color Error = new(220, 40, 40, 255);
    public static readonly Color Info = new(0, 140, 255, 255);

    // Font sizes
    public const int TitleFont = 22;
    public const int HeaderFont = 18;
    public const int BodyFont = 15;
    public const int SmallFont = 12;

    // Spacing
    public const int Padding = 8;
    public const int Margin = 12;
} 