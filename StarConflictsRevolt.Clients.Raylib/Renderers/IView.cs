namespace StarConflictsRevolt.Clients.Raylib.Renderers;

public interface IView
{
    GameView ViewType { get; }
    void Draw();
}
