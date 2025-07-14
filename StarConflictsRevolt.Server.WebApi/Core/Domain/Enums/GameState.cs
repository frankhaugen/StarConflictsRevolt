namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Enums;

public class GameState
{
    public int CurrentTurn { get; set; }
    public World.World World { get; set; } = new();
}