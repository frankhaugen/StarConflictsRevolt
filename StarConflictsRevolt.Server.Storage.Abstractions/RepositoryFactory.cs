using Microsoft.Extensions.DependencyInjection;

namespace StarConflictsRevolt.Server.Storage.Abstractions;

internal sealed class RepositoryFactory(IServiceProvider sp) : IRepositoryFactory
{
    public IRepository<T> Create<T>() where T : class, IHasId
        => sp.GetRequiredService<IRepository<T>>();
}
