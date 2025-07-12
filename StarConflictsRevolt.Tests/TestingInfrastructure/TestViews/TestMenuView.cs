using StarConflictsRevolt.Clients.Raylib.Renderers;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

public class TestMenuView : IView
{
    public GameView ViewType => GameView.Menu;
    public void Draw() { }
}