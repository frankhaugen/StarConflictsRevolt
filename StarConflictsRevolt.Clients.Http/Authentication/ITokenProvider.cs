namespace StarConflictsRevolt.Clients.Http.Authentication;

public interface ITokenProvider
{
    Task<string> GetTokenAsync(CancellationToken ct = default);
    Task<string?> GetCachedTokenAsync(CancellationToken ct = default);
    Task InvalidateTokenAsync(CancellationToken ct = default);
} 