using StarConflictsRevolt.Server.WebApi.Core.Domain.Enums;

namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Gameplay;

public class PlayerStats : IGameObject
{
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

    /// <inheritdoc />
    public Guid Id { get; set; }
}