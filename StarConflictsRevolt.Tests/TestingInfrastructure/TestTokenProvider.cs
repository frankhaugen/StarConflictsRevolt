using StarConflictsRevolt.Clients.Http.Authentication;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

public class TestTokenProvider : ITokenProvider
{
    public Task<string?> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<string?>("test-token");
    }

    /// <inheritdoc />
    public async Task<string?> GetCachedTokenAsync(CancellationToken ct = default)
    {
        return null;
    }

    /// <inheritdoc />
    public async Task InvalidateTokenAsync(CancellationToken ct = default)
    {
    }
} 