namespace StarConflictsRevolt.Clients.Raylib.Renderers;

public enum GameView
{
    // Main navigation views
    Menu,
    Galaxy,
    System,
    Planet,
    
    // Strategic & Finder Views (F1-F7)
    GameOptions,
    PlanetaryFinder,
    FleetFinder,
    TroopFinder,
    PersonnelFinder,
    MessageWindow,
    Encyclopedia,
    
    // Agent menu / GID Views (ALT-A to ALT-V)
    BuildShips,
    BuildTroops,
    BuildFacilities,
    GalaxyOverview,
    GameObjectives,
    ManageGarrisons,
    ManageProduction,
    TranslateCounterpart,
    AgentAdvice,
    PopularSupport,
    Uprising,
    IdleFleets,
    EnrouteFleets,
    IdlePersonnel,
    ActivePersonnel,
    IdleShipyard,
    IdleTrainingFacilities,
    IdleConstruction,
    
    // Tactical / Battle Views
    TacticalBattle,
    DeathStarControl,
    
    // System & Miscellaneous Windows
    SaveLoadScreen,
    MultiplayerOptions,
    PauseDialog
}

