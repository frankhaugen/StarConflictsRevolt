namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

/// <summary>
/// Strongly typed game timestamp to avoid using primitive DateTime
/// </summary>
public record GameTimestamp(DateTime Value)
{
    public static implicit operator DateTime(GameTimestamp timestamp) => timestamp.Value;
    public static implicit operator GameTimestamp(DateTime value) => new(value);
    
    public override string ToString() => Value.ToString("O");
}