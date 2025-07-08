using System.Text.Json;
using System.Text.Json.Serialization;
using StarConflictsRevolt.Clients.Models;

namespace StarConflictsRevolt.Clients.Shared;

public class ClientWorldStore : IClientWorldStore
{
    WorldDto? _current;
    readonly List<WorldDto> _history = new();

    public IReadOnlyList<WorldDto> History => _history;

    /// <inheritdoc />
    public SessionDto? Session { get; set; }

    public void ApplyFull(WorldDto world)
    {
        _current = world with { Galaxy = world.Galaxy with {
            StarSystems = new List<StarSystemDto>(world.Galaxy.StarSystems)
        }};
        Snapshot();
    }

    public void ApplyDeltas(IEnumerable<GameObjectUpdate> deltas)
    {
        if (_current is null) return;

        var starSystems = _current.Galaxy.StarSystems.ToList();
        
        var planets = starSystems.SelectMany(x => x.Planets).ToList();
        foreach (var d in deltas)
        {
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
        }
        Snapshot();
    }

    public WorldDto GetCurrent()
    {
        if (_current is null)
            throw new InvalidOperationException("World not initialized");
        return _current with {
            Galaxy = _current.Galaxy with {
                StarSystems = _current.Galaxy.StarSystems.Select(system => system with {
                    Planets = system.Planets.ToList() // Ensure we have a mutable list
                }).ToList()
            }
        };
    }

    void Snapshot()
    {
        _history.Add(GetCurrent());
        if (_history.Count > 100)
            _history.RemoveAt(0);
    }

    static readonly JsonSerializerOptions _jsonOptions = new() {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}