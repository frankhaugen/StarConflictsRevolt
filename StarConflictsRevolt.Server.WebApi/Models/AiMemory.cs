namespace StarConflictsRevolt.Server.WebApi.Models;

public enum MemoryType
{
    Decision,
    Outcome,
    Threat,
    Opportunity,
    PlayerBehavior,
    ResourceState,
    CombatResult
}

public class AiMemory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlayerId { get; set; }
    public MemoryType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
    public double Importance { get; set; } // 0.0 to 1.0
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime LastAccessed { get; set; } = DateTime.UtcNow;
    public int AccessCount { get; set; }
    public bool IsForgotten { get; set; }

    public AiMemory(Guid playerId, MemoryType type, string description, double importance = 0.5)
    {
        PlayerId = playerId;
        Type = type;
        Description = description;
        Importance = Math.Clamp(importance, 0.0, 1.0);
    }

    public void AddData(string key, object value)
    {
        Data[key] = value;
    }

    public T? GetData<T>(string key)
    {
        if (Data.TryGetValue(key, out var value) && value is T typedValue)
        {
            Access();
            return typedValue;
        }
        return default;
    }

    public void Access()
    {
        LastAccessed = DateTime.UtcNow;
        AccessCount++;
    }

    public void Forget()
    {
        IsForgotten = true;
    }

    public bool IsExpired(TimeSpan maxAge)
    {
        return DateTime.UtcNow - LastAccessed > maxAge;
    }

    public double GetRelevance()
    {
        // Calculate relevance based on importance, recency, and access frequency
        var ageInHours = (DateTime.UtcNow - LastAccessed).TotalHours;
        var recencyFactor = Math.Exp(-ageInHours / 24.0); // Decay over 24 hours
        var frequencyFactor = Math.Min(AccessCount / 10.0, 1.0); // Cap at 10 accesses
        
        return Importance * (0.7 * recencyFactor + 0.3 * frequencyFactor);
    }

    public override string ToString()
    {
        var relevance = GetRelevance();
        return $"{Type} - {Description} [Importance: {Importance:F2}, Relevance: {relevance:F2}, Accesses: {AccessCount}]";
    }
}

public class AiMemoryBank
{
    private readonly List<AiMemory> _memories = new();
    private readonly int _maxMemories = 1000;
    private readonly object _lock = new();

    public void AddMemory(AiMemory memory)
    {
        lock (_lock)
        {
            _memories.Add(memory);
            
            // Clean up old memories if we exceed the limit
            if (_memories.Count > _maxMemories)
            {
                CleanupOldMemories();
            }
        }
    }

    public List<AiMemory> GetMemories(Guid playerId, MemoryType? type = null, int limit = 50)
    {
        lock (_lock)
        {
            var query = _memories
                .Where(m => m.PlayerId == playerId && !m.IsForgotten);

            if (type.HasValue)
            {
                query = query.Where(m => m.Type == type.Value);
            }

            return query
                .OrderByDescending(m => m.GetRelevance())
                .Take(limit)
                .ToList();
        }
    }

    public List<AiMemory> GetRecentMemories(Guid playerId, TimeSpan maxAge, int limit = 20)
    {
        lock (_lock)
        {
            return _memories
                .Where(m => m.PlayerId == playerId && 
                           !m.IsForgotten && 
                           DateTime.UtcNow - m.Created <= maxAge)
                .OrderByDescending(m => m.Created)
                .Take(limit)
                .ToList();
        }
    }

    public void ForgetOldMemories(TimeSpan maxAge)
    {
        lock (_lock)
        {
            var cutoff = DateTime.UtcNow - maxAge;
            foreach (var memory in _memories.Where(m => m.Created < cutoff))
            {
                memory.Forget();
            }
        }
    }

    private void CleanupOldMemories()
    {
        // Remove the least relevant memories
        var memoriesToRemove = _memories
            .Where(m => !m.IsForgotten)
            .OrderBy(m => m.GetRelevance())
            .Take(_memories.Count - _maxMemories + 100); // Remove extra to make room

        foreach (var memory in memoriesToRemove)
        {
            memory.Forget();
        }
    }

    public int GetMemoryCount(Guid playerId)
    {
        lock (_lock)
        {
            return _memories.Count(m => m.PlayerId == playerId && !m.IsForgotten);
        }
    }

    public void ClearMemories(Guid playerId)
    {
        lock (_lock)
        {
            foreach (var memory in _memories.Where(m => m.PlayerId == playerId))
            {
                memory.Forget();
            }
        }
    }
} 