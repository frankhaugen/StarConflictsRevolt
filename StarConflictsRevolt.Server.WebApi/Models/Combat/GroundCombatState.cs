namespace StarConflictsRevolt.Server.WebApi.Models.Combat;

public class GroundCombatState
{
    public int CurrentRound { get; set; } = 0;
    public int MaxRounds { get; set; } = 15;
    public TerrainType Terrain { get; set; } = TerrainType.Planetary;
    public WeatherCondition Weather { get; set; } = WeatherCondition.Clear;
    public bool IsNight { get; set; } = false;
    public bool HasCover { get; set; } = false;
    
    public double GetTerrainModifier()
    {
        return Terrain switch
        {
            TerrainType.Planetary => 1.0,
            TerrainType.Urban => 0.8, // Urban combat is more difficult
            TerrainType.Forest => 0.7, // Forest provides cover
            TerrainType.Desert => 1.2, // Open terrain
            TerrainType.Mountain => 0.6, // Mountainous terrain
            _ => 1.0
        };
    }
    
    public double GetWeatherModifier()
    {
        return Weather switch
        {
            WeatherCondition.Clear => 1.0,
            WeatherCondition.Storm => 0.6,
            WeatherCondition.Fog => 0.7,
            WeatherCondition.Radiation => 0.8,
            _ => 1.0
        };
    }
    
    public double GetVisibilityModifier()
    {
        var modifier = 1.0;
        
        if (IsNight) modifier *= 0.5;
        if (HasCover) modifier *= 0.8;
        
        return modifier * GetWeatherModifier();
    }
}