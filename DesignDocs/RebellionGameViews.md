I added **RebellionViews.cs** to the canvas with one concise C# record for every screen listed in the manual’s keyboard-shortcut tables and window descriptions.
Each record inherits from a tiny `GameView` base class that stores the user-visible title and implements the `IGameView` marker interface, so you can flesh out view-specific properties later without losing type safety across the UI layer.

```csharp
using System;

namespace StarConflictsRevolt.Client.Views;

/// <summary>
/// Marker interface implemented by every view/page in the StarConflictsRevolt client.
/// </summary>
public interface IGameView
{
    /// <summary>User‑visible title (localised in the UI layer).</summary>
    string Title { get; }
}

/// <summary>
/// Convenience abstract record that stores a <see cref="Title"/>. All concrete view‑models inherit from it.
/// </summary>
public abstract record GameView(string Title) : IGameView;

#region Strategic & Finder Views (keyboard shortcuts F1‑F7) – manual pp. 18‑37
public record GameOptionsView()      : GameView("Game Options");
public record PlanetaryFinderView()  : GameView("Planetary Finder");
public record FleetFinderView()      : GameView("Fleet/Ship Finder");
public record TroopFinderView()      : GameView("Troop Finder");
public record PersonnelFinderView()  : GameView("Personnel Finder");
public record MessageWindowView()    : GameView("Message Window");
public record EncyclopediaView()     : GameView("Encyclopedia");
#endregion

#region Agent‑menu / GID Views (ALT‑A … ALT‑V) – see README ‘Agent Menu’ list
public record BuildShipsView()            : GameView("Build Ships");
public record BuildTroopsView()           : GameView("Build Troops");
public record BuildFacilitiesView()       : GameView("Build Facilities");
public record GalaxyOverviewView()        : GameView("Galaxy Overview");
public record GameObjectivesView()        : GameView("Game Objectives");
public record ManageGarrisonsView()       : GameView("Manage Garrisons");
public record ManageProductionView()      : GameView("Manage Production");
public record TranslateCounterpartView()  : GameView("Translate Counterpart");
public record AgentAdviceView()           : GameView("Agent Advice");
public record PopularSupportView()        : GameView("Popular Support");
public record UprisingView()              : GameView("Uprising");
public record IdleFleetsView()            : GameView("Idle Fleets");
public record EnrouteFleetsView()         : GameView("En‑route Fleets");
public record IdlePersonnelView()         : GameView("Idle Personnel");
public record ActivePersonnelView()       : GameView("Active Personnel");
public record IdleShipyardView()          : GameView("Idle Shipyard");
public record IdleTrainingFacilitiesView(): GameView("Idle Training Facilities");
public record IdleConstructionView()      : GameView("Idle Construction");
#endregion

#region Tactical / Battle Views – manual pp. 96‑110
public record TacticalBattleView()  : GameView("Tactical Battle");
public record DeathStarControlView(): GameView("Death Star Control");
#endregion

#region System & Miscellaneous Windows
public record SaveLoadScreenView()     : GameView("Save/Load Screen");
public record MultiplayerOptionsView() : GameView("Multiplayer Options");
public record PauseDialogView()        : GameView("Pause Dialog");
#endregion
```

Let me know if you’d like extra details (e.g., initial DTO fields for particular panels, an enum, or grouping these into separate files).
