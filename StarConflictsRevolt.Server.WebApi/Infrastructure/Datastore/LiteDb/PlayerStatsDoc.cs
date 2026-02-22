using StarConflictsRevolt.Server.Domain.Gameplay;

namespace StarConflictsRevolt.Server.WebApi.Infrastructure.Datastore.LiteDb;

internal sealed class PlayerStatsDoc
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public Guid SessionId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int FleetsOwned { get; set; }
    public int PlanetsControlled { get; set; }
    public int StructuresBuilt { get; set; }
    public int BattlesWon { get; set; }
    public int BattlesLost { get; set; }
    public DateTime LastUpdated { get; set; }
    public DateTime Created { get; set; }

    public static PlayerStatsDoc From(PlayerStats ps)
    {
        return new PlayerStatsDoc
        {
            Id = ps.Id,
            PlayerId = ps.PlayerId,
            SessionId = ps.SessionId,
            PlayerName = ps.PlayerName,
            FleetsOwned = ps.FleetsOwned,
            PlanetsControlled = ps.PlanetsControlled,
            StructuresBuilt = ps.StructuresBuilt,
            BattlesWon = ps.BattlesWon,
            BattlesLost = ps.BattlesLost,
            LastUpdated = ps.LastUpdated,
            Created = ps.Created
        };
    }

    public PlayerStats ToPlayerStats()
    {
        return new PlayerStats
        {
            Id = Id,
            PlayerId = PlayerId,
            SessionId = SessionId,
            PlayerName = PlayerName,
            FleetsOwned = FleetsOwned,
            PlanetsControlled = PlanetsControlled,
            StructuresBuilt = StructuresBuilt,
            BattlesWon = BattlesWon,
            BattlesLost = BattlesLost,
            LastUpdated = LastUpdated,
            Created = Created
        };
    }
}
