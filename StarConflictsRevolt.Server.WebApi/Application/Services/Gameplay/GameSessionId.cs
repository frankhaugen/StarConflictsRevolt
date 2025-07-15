namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

/// <summary>
/// Strongly typed session ID to avoid using primitive Guid
/// </summary>
public record GameSessionId(Guid Value)
{
    public static implicit operator Guid(GameSessionId sessionId) => sessionId.Value;
    public static implicit operator GameSessionId(Guid value) => new(value);
    
    public override string ToString() => Value.ToString();
    
    public static GameSessionId New() => new(Guid.NewGuid());
}