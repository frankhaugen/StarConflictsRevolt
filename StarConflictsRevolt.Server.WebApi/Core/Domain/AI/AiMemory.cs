namespace StarConflictsRevolt.Server.WebApi.Core.Domain.AI;

public class AiMemory
{
    public AiMemory(Guid playerId, MemoryType type, string description, double importance = 0.5)
    {
        PlayerId = playerId;
        Type = type;
        Description = description;
        Importance = Math.Clamp(importance, 0.0, 1.0);
    }

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