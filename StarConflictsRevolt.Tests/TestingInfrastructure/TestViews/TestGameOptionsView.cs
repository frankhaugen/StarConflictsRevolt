using StarConflictsRevolt.Clients.Raylib.Renderers;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

public class TestGameOptionsView : IView
{
    public GameView ViewType => GameView.GameOptions;
    public void Draw() { }
}