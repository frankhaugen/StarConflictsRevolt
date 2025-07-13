using StarConflictsRevolt.Clients.Raylib.Renderers;

namespace StarConflictsRevolt.Tests.TestingInfrastructure.TestViews;

public class TestGameOptionsView : IView
{
    public GameView ViewType => GameView.GameOptions;
    public void Draw() { }
}