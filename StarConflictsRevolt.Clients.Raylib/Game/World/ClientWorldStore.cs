using System.Text.Json;
using System.Text.Json.Serialization;
using StarConflictsRevolt.Clients.Models;

namespace StarConflictsRevolt.Clients.Raylib.Game.World;

public class ClientWorldStore(ILogger<ClientWorldStore> logger) : IClientWorldStore
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly List<WorldDto?> _history = new();
    private WorldDto? _current;

    public IReadOnlyList<WorldDto?> History => _history;

    /// <inheritdoc />
    public SessionDto? Session { get; set; }

    public void ApplyFull(WorldDto? world)
    {
        logger.LogInformation("ApplyFull called with world: {WorldId}, StarSystems: {StarSystemCount}",
            world?.Id, world?.Galaxy?.StarSystems?.Count() ?? 0);

        if (world == null)
        {
            logger.LogWarning("ApplyFull called with null world");
            _current = null;
            return;
        }

        if (world.Galaxy == null)
        {
            logger.LogWarning("ApplyFull called with world that has null Galaxy");
            _current = world;
            return;
        }

        if (world.Galaxy.StarSystems == null)
        {
            logger.LogWarning("ApplyFull called with world that has null StarSystems");
            _current = world;
            return;
        }

        _current = world with
        {
            Galaxy = world.Galaxy with
            {
                StarSystems = new List<StarSystemDto>(world.Galaxy.StarSystems)
            }
        };

        logger.LogInformation("World applied successfully. Current world: {WorldId}, StarSystems: {StarSystemCount}",
            _current.Id, _current.Galaxy?.StarSystems?.Count() ?? 0);

        Snapshot();
    }

    public void ApplyDeltas(IEnumerable<GameObjectUpdate> deltas)
    {
        logger.LogInformation("ApplyDeltas called with {DeltaCount} deltas", deltas.Count());
        if (_current is null) return;
        if (_current.Galaxy is null || _current.Galaxy.StarSystems is null)
        {
            logger.LogWarning("Current world does not have a valid Galaxy or StarSystems");
            return;
        }

        var starSystems = _current.Galaxy.StarSystems.ToList();

        var planets = starSystems.SelectMany(x => x.Planets).ToList();
        foreach (var d in deltas)
            switch (d.Type)
            {
                case UpdateType.Added:
                case UpdateType.Changed:
                    var dto = d.Data?.Deserialize<PlanetDto>(_jsonOptions);
                    if (dto != null)
                    {
                        var idx = planets.FindIndex(p => p.Id == d.Id);
                        if (idx >= 0) planets[idx] = dto;
                        else planets.Add(dto);
                    }

                    break;
                case UpdateType.Removed:
                    planets.RemoveAll(p => p.Id == d.Id);
                    break;
            }

        Snapshot();
    }

    public WorldDto? GetCurrent()
    {
        logger.LogDebug("GetCurrent called. Current world: {WorldId}, Has Galaxy: {HasGalaxy}, StarSystems: {StarSystemCount}",
            _current?.Id, _current?.Galaxy != null, _current?.Galaxy?.StarSystems?.Count() ?? 0);

        if (_current is null)
            return null;
        return _current with
        {
            Galaxy = _current.Galaxy with
            {
                StarSystems = _current.Galaxy.StarSystems.Select(system => system with
                {
                    Planets = system.Planets.ToList() // Ensure we have a mutable list
                }).ToList()
            }
        };
    }

    private void Snapshot()
    {
        if (_current is null)
        {
            logger.LogWarning("Snapshot called but current world is null");
            return;
        }

        logger.LogInformation("Snapshotting current world: {WorldId}, StarSystems: {StarSystemCount}",
            _current.Id, _current.Galaxy?.StarSystems?.Count() ?? 0);
        _history.Add(GetCurrent());
        if (_history.Count > 100)
            _history.RemoveAt(0);
    }
}