using StarConflictsRevolt.Server.WebApi.Models;

namespace StarConflictsRevolt.Server.WebApi.Datastore.Entities;

public class Session : IGameObject
{
    /// <inheritdoc />
    public Guid Id { get; set; }
    
    public string SessionName { get; set; }
    
    public DateTime Created { get; set; }
    
    public bool IsActive { get; set; }
    
    public DateTime? Ended { get; set; }
    
    public SessionType SessionType { get; set; }
}