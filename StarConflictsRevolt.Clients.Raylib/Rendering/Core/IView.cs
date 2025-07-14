namespace StarConflictsRevolt.Clients.Raylib.Rendering.Core;

public interface IView
{
    GameView ViewType { get; }
    void Draw();
}