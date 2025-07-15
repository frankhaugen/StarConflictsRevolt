namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

/// <summary>
/// Strongly typed player ID to avoid using primitive Guid
/// </summary>
public record GamePlayerId(Guid Value)
{
    public static implicit operator Guid(GamePlayerId playerId) => playerId.Value;
    public static implicit operator GamePlayerId(Guid value) => new(value);
    
    public override string ToString() => Value.ToString();
    
    public static GamePlayerId New() => new(Guid.NewGuid());
}