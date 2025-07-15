namespace StarConflictsRevolt.Clients.Http.Authentication;

public interface ITokenProvider
{
    Task<string> GetTokenAsync(CancellationToken ct = default);
}