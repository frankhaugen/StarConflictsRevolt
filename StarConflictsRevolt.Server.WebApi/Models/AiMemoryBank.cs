namespace StarConflictsRevolt.Server.WebApi.Models;

public class AiMemoryBank
{
    private readonly object _lock = new();
    private readonly int _maxMemories = 1000;
    private readonly List<AiMemory> _memories = new();

    public void AddMemory(AiMemory memory)
    {
        lock (_lock)
        {
            _memories.Add(memory);

            // Clean up old memories if we exceed the limit
            if (_memories.Count > _maxMemories) CleanupOldMemories();
        }
    }

    public List<AiMemory> GetMemories(Guid playerId, MemoryType? type = null, int limit = 50)
    {
        lock (_lock)
        {
            var query = _memories
                .Where(m => m.PlayerId == playerId && !m.IsForgotten);

            if (type.HasValue) query = query.Where(m => m.Type == type.Value);

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
            foreach (var memory in _memories.Where(m => m.Created < cutoff)) memory.Forget();
        }
    }

    private void CleanupOldMemories()
    {
        // Remove the least relevant memories
        var memoriesToRemove = _memories
            .Where(m => !m.IsForgotten)
            .OrderBy(m => m.GetRelevance())
            .Take(_memories.Count - _maxMemories + 100); // Remove extra to make room

        foreach (var memory in memoriesToRemove) memory.Forget();
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
            foreach (var memory in _memories.Where(m => m.PlayerId == playerId)) memory.Forget();
        }
    }
}