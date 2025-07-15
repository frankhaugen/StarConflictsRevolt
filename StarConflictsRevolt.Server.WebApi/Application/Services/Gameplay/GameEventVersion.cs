namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

/// <summary>
/// Strongly typed event version to avoid using primitive int
/// </summary>
public record GameEventVersion(int Value)
{
    public static implicit operator int(GameEventVersion version) => version.Value;
    public static implicit operator GameEventVersion(int value) => new(value);
    
    public override string ToString() => Value.ToString();
}