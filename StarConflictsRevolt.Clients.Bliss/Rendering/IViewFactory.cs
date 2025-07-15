using StarConflictsRevolt.Clients.Bliss.Core;

namespace StarConflictsRevolt.Clients.Bliss.Rendering;

/// <summary>
/// Factory for creating game views.
/// </summary>
public interface IViewFactory
{
    /// <summary>
    /// Creates a view of the specified type.
    /// </summary>
    /// <param name="viewType">The type of view to create</param>
    /// <returns>The created view</returns>
    IView CreateView(GameView viewType);
} 