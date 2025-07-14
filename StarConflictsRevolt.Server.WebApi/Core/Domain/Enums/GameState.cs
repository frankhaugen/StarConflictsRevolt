namespace StarConflictsRevolt.Server.WebApi.Models;

public class GameState
{
    public int CurrentTurn { get; set; }
    public World World { get; set; } = new();
}