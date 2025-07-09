namespace StarConflictsRevolt.Clients.Shared;

public interface ITokenProvider
{
    Task<string> GetTokenAsync(CancellationToken ct = default);
}