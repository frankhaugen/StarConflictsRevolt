using StarConflictsRevolt.Clients.Blazor.Services;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

/// <summary>
/// Test double for IClientSessionStorage (in-memory; no JS/sessionStorage/localStorage).
/// </summary>
public sealed class FakeClientSessionStorage : IClientSessionStorage
{
    private Guid? _sessionId;
    private string? _playerName;

    public Task<Guid?> GetSessionIdAsync(CancellationToken cancellationToken = default) => Task.FromResult(_sessionId);
    public Task SetSessionIdAsync(Guid? sessionId, CancellationToken cancellationToken = default)
    {
        _sessionId = sessionId;
        return Task.CompletedTask;
    }
    public Task<string?> GetPlayerNameAsync(CancellationToken cancellationToken = default) => Task.FromResult(_playerName);
    public Task SetPlayerNameAsync(string? playerName, CancellationToken cancellationToken = default)
    {
        _playerName = string.IsNullOrWhiteSpace(playerName) ? null : playerName.Trim();
        return Task.CompletedTask;
    }
}
