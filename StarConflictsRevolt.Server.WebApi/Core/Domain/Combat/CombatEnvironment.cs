namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Combat;

public class CombatEnvironment
{
    public TerrainType Terrain { get; set; } = TerrainType.Space;
    public WeatherCondition Weather { get; set; } = WeatherCondition.Clear;
    public double Visibility { get; set; } = 1.0;
    public double Gravity { get; set; } = 1.0;
    public bool HasAtmosphere { get; set; } = false;

    public double GetAccuracyModifier()
    {
        var modifier = 1.0;

        // Weather effects
        modifier *= Weather switch
        {
            WeatherCondition.Storm => 0.7,
            WeatherCondition.Fog => 0.8,
            WeatherCondition.Clear => 1.0,
            _ => 1.0
        };

        // Visibility effects
        modifier *= Visibility;

        return modifier;
    }
}