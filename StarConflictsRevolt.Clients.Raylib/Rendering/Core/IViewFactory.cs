namespace StarConflictsRevolt.Clients.Raylib.Rendering.Core;

public interface IViewFactory
{
    IView CreateView(GameView viewType);
}