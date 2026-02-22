namespace StarConflictsRevolt.Server.Domain.Enums;

public class GameState
{
    public int CurrentTurn { get; set; }
    public World.World World { get; set; } = new();
}