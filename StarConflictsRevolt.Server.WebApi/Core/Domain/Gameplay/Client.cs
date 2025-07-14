namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Gameplay;

public class Client
{
    public string Id { get; set; } = string.Empty;
    public DateTime LastSeen { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Session> Sessions { get; set; } = new List<Session>();
}