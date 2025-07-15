using Raylib_CSharp.Fonts;

namespace StarConflictsRevolt.Clients.Raylib.Rendering.UI;

public static class FontHelper
{
    private const int FNT = 48; // Default font size, can be adjusted as needed
    private const string FontPath = "Assets/Fonts/";
    
    private static string GetPath(string fileName) => Path.Combine(FontPath, fileName);

    
    public static Font Galaxy { get; private set; } = Font.LoadEx(GetPath("Galaxy.ttf"), FNT, ReadOnlySpan<int>.Empty);
    
    public static Font Spaceman { get; private set; } = Font.LoadEx(GetPath("Spaceman.ttf"), FNT, ReadOnlySpan<int>.Empty);
    
    public static Font NeuropolX { get; private set; } = Font.LoadEx(GetPath("NeuropolX.otf"), FNT, ReadOnlySpan<int>.Empty);
}