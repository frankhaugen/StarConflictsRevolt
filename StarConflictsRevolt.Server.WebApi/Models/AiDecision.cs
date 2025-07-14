namespace StarConflictsRevolt.Server.WebApi.Models;

public class AiDecision
{
    public AiDecision(AiDecisionType type, AiPriority priority, double score, string description)
    {
        Type = type;
        Priority = priority;
        Score = score;
        Description = description;
    }

    public Guid Id { get; set; } = Guid.NewGuid();
    public AiDecisionType Type { get; set; }
    public AiPriority Priority { get; set; }
    public double Score { get; set; }
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public bool IsExecuted { get; set; }
    public DateTime? ExecutedAt { get; set; }
    public bool IsSuccessful { get; set; }

    public void AddParameter(string key, object value)
    {
        Parameters[key] = value;
    }

    public T? GetParameter<T>(string key)
    {
        if (Parameters.TryGetValue(key, out var value) && value is T typedValue) return typedValue;
        return default;
    }

    public void MarkExecuted(bool successful = true)
    {
        IsExecuted = true;
        ExecutedAt = DateTime.UtcNow;
        IsSuccessful = successful;
    }

    public override string ToString()
    {
        return $"{Type} ({Priority}) - {Description} [Score: {Score:F2}]";
    }
}