namespace StarConflictsRevolt.Clients.Raylib.Http;

public interface ITokenProvider
{
    Task<string> GetTokenAsync(CancellationToken ct = default);
}