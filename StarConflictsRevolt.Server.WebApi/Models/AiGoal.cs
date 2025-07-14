namespace StarConflictsRevolt.Server.WebApi.Models;

public class AiGoal
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public AiGoalType Type { get; set; }
    public GoalTimeframe Timeframe { get; set; }
    public string Description { get; set; } = string.Empty;
    public double Priority { get; set; }
    public double Progress { get; set; } // 0.0 to 1.0
    public bool IsCompleted { get; set; }
    public bool IsAbandoned { get; set; }
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
    public List<AiDecision> RelatedDecisions { get; set; } = new();

    public AiGoal(AiGoalType type, GoalTimeframe timeframe, string description, double priority)
    {
        Type = type;
        Timeframe = timeframe;
        Description = description;
        Priority = priority;
    }

    public void UpdateProgress(double newProgress)
    {
        Progress = Math.Clamp(newProgress, 0.0, 1.0);
        if (Progress >= 1.0 && !IsCompleted)
        {
            Complete();
        }
    }

    public void Complete()
    {
        IsCompleted = true;
        CompletedAt = DateTime.UtcNow;
        Progress = 1.0;
    }

    public void Abandon()
    {
        IsAbandoned = true;
    }

    public void AddParameter(string key, object value)
    {
        Parameters[key] = value;
    }

    public T? GetParameter<T>(string key)
    {
        if (Parameters.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return default;
    }

    public void AddRelatedDecision(AiDecision decision)
    {
        RelatedDecisions.Add(decision);
    }

    public bool IsExpired()
    {
        var maxAge = Timeframe switch
        {
            GoalTimeframe.Immediate => TimeSpan.FromMinutes(1),
            GoalTimeframe.ShortTerm => TimeSpan.FromMinutes(5),
            GoalTimeframe.MediumTerm => TimeSpan.FromMinutes(20),
            GoalTimeframe.LongTerm => TimeSpan.FromHours(2),
            _ => TimeSpan.FromHours(1)
        };

        return DateTime.UtcNow - Created > maxAge;
    }

    public override string ToString()
    {
        var status = IsCompleted ? "COMPLETED" : IsAbandoned ? "ABANDONED" : $"PROGRESS: {Progress:P0}";
        return $"{Type} ({Timeframe}) - {Description} [{status}] [Priority: {Priority:F2}]";
    }
} 