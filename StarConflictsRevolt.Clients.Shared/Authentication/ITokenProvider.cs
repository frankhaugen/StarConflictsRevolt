namespace StarConflictsRevolt.Clients.Shared.Authentication;

public interface ITokenProvider
{
    Task<string> GetTokenAsync(CancellationToken ct = default);
}