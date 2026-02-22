using StarConflictsRevolt.Clients.Blazor.Services;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

/// <summary>
/// Test double for IClientIdProvider that returns a fixed id (no JS/localStorage).
/// </summary>
public sealed class FakeClientIdProvider : IClientIdProvider
{
    private readonly string _clientId;

    public FakeClientIdProvider(string clientId = "test-client-id")
    {
        _clientId = clientId;
    }

    public Task<string> GetClientIdAsync(CancellationToken cancellationToken = default) => Task.FromResult(_clientId);
}
