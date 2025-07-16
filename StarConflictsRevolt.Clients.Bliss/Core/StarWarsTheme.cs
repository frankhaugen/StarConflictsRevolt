using Color = Bliss.CSharp.Colors.Color;

namespace StarConflictsRevolt.Clients.Bliss.Core;

/// <summary>
/// Star Wars Rebellion inspired color theme and styling constants.
/// </summary>
public static class StarWarsTheme
{
    // Empire Colors (Red/Gray theme)
    public static readonly Color EmpirePrimary = new Color(204, 26, 26, 255);      // Dark Red
    public static readonly Color EmpireSecondary = new Color(102, 102, 102, 255);    // Gray
    public static readonly Color EmpireAccent = new Color(255, 51, 51, 255);       // Bright Red
    
    // Rebellion Colors (Blue/White theme)
    public static readonly Color RebellionPrimary = new Color(26, 77, 204, 255);   // Blue
    public static readonly Color RebellionSecondary = new Color(204, 204, 255, 255); // Light Blue
    public static readonly Color RebellionAccent = new Color(51, 153, 255, 255);    // Bright Blue
    
    // Neutral Colors
    public static readonly Color Neutral = new Color(128, 128, 128, 255);           // Gray
    public static readonly Color Background = new Color(13, 13, 26, 255);      // Dark Space Blue
    public static readonly Color BackgroundSecondary = new Color(26, 26, 38, 255); // Slightly lighter
    
    // UI Colors
    public static readonly Color PanelBackground = new Color(38, 38, 51, 230);  // Semi-transparent panel
    public static readonly Color Border = new Color(77, 77, 102, 255);            // Panel border
    public static readonly Color Text = new Color(230, 230, 230, 255);             // Light text
    public static readonly Color TextSecondary = new Color(179, 179, 179, 255);     // Secondary text
    
    // Status Colors
    public static readonly Color Success = new Color(51, 204, 51, 255);          // Green
    public static readonly Color Warning = new Color(255, 204, 51, 255);          // Yellow
    public static readonly Color Danger = new Color(204, 51, 51, 255);           // Red
    public static readonly Color Info = new Color(51, 153, 255, 255);             // Blue
    
    // Star field colors for background effects
    public static readonly Color StarBright = new Color(255, 255, 255, 255);       // White
    public static readonly Color StarDim = new Color(153, 153, 204, 255);          // Dim blue-white
    public static readonly Color StarDistant = new Color(77, 77, 128, 255);      // Very dim
    
    /// <summary>
    /// Get faction colors based on allegiance.
    /// </summary>
    public static (Color primary, Color secondary, Color accent) GetFactionColors(Faction faction)
    {
        return faction switch
        {
            Faction.Empire => (EmpirePrimary, EmpireSecondary, EmpireAccent),
            Faction.Alliance => (RebellionPrimary, RebellionSecondary, RebellionAccent),
            _ => (Neutral, Neutral, Neutral)
        };
    }
}

/// <summary>
/// Faction enumeration for the game.
/// </summary>
public enum Faction
{
    Empire,
    Alliance,
    Neutral
} 