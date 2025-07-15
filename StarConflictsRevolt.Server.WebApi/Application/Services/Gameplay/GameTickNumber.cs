namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

/// <summary>
/// Strongly typed tick number to avoid using primitive long
/// </summary>
public record GameTickNumber(long Value)
{
    public static implicit operator long(GameTickNumber tickNumber) => tickNumber.Value;
    public static implicit operator GameTickNumber(long value) => new(value);
    
    public override string ToString() => Value.ToString();
}