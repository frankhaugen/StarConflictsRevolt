using StarConflictsRevolt.Server.Storage.Abstractions;

namespace StarConflictsRevolt.Server.Storage.InMemory;

internal sealed class InMemoryRepositoryProvider(IInMemoryStore store) : IRepositoryProvider
{
    public IRepository<T> Create<T>() where T : class, IHasId => new InMemoryRepository<T>(store);
}
