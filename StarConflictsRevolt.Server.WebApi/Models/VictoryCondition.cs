namespace StarConflictsRevolt.Server.WebApi.Models;

public class VictoryCondition
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public VictoryType Type { get; set; }
    
    // Victory requirements
    public int RequiredPlanetPercentage { get; set; } = 75;
    public int RequiredCredits { get; set; } = 10000;
    public int RequiredTechnologies { get; set; } = 15;
    public int RequiredTurns { get; set; } = 100;
    
    // Check if a player has achieved this victory condition
    public bool IsAchieved(PlayerState playerState, GameState gameState)
    {
        return Type switch
        {
            VictoryType.Military => CheckMilitaryVictory(playerState, gameState),
            VictoryType.Economic => CheckEconomicVictory(playerState),
            VictoryType.Technology => CheckTechnologyVictory(playerState),
            VictoryType.Time => CheckTimeVictory(gameState),
            VictoryType.Diplomatic => CheckDiplomaticVictory(playerState, gameState),
            _ => false
        };
    }
    
    private bool CheckMilitaryVictory(PlayerState playerState, GameState gameState)
    {
        var totalPlanets = gameState.World.Galaxy.StarSystems
            .SelectMany(s => s.Planets)
            .Count();
        
        var controlledPlanets = gameState.World.Galaxy.StarSystems
            .SelectMany(s => s.Planets)
            .Count(p => p.OwnerId == playerState.PlayerId);
        
        var percentage = totalPlanets > 0 ? (double)controlledPlanets / totalPlanets * 100 : 0;
        return percentage >= RequiredPlanetPercentage;
    }
    
    private bool CheckEconomicVictory(PlayerState playerState)
    {
        return playerState.Credits >= RequiredCredits;
    }
    
    private bool CheckTechnologyVictory(PlayerState playerState)
    {
        return playerState.ResearchedTechnologies.Count >= RequiredTechnologies;
    }
    
    private bool CheckTimeVictory(GameState gameState)
    {
        return gameState.CurrentTurn >= RequiredTurns;
    }
    
    private bool CheckDiplomaticVictory(PlayerState playerState, GameState gameState)
    {
        // For now, diplomatic victory is not implemented
        // Would require alliance system and voting mechanics
        return false;
    }
    
    // Static victory condition definitions
    public static VictoryCondition MilitaryVictory => new()
    {
        Name = "Military Victory",
        Description = "Control 75% of all planets in the galaxy",
        Type = VictoryType.Military,
        RequiredPlanetPercentage = 75
    };
    
    public static VictoryCondition EconomicVictory => new()
    {
        Name = "Economic Victory",
        Description = "Accumulate 10,000 credits",
        Type = VictoryType.Economic,
        RequiredCredits = 10000
    };
    
    public static VictoryCondition TechnologyVictory => new()
    {
        Name = "Technology Victory",
        Description = "Research 15 different technologies",
        Type = VictoryType.Technology,
        RequiredTechnologies = 15
    };
    
    public static VictoryCondition TimeVictory => new()
    {
        Name = "Time Victory",
        Description = "Survive for 100 turns",
        Type = VictoryType.Time,
        RequiredTurns = 100
    };
    
    public static VictoryCondition DiplomaticVictory => new()
    {
        Name = "Diplomatic Victory",
        Description = "Form alliances with other players (not implemented)",
        Type = VictoryType.Diplomatic
    };
}

// Helper classes for victory condition checking