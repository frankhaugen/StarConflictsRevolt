using StarConflictsRevolt.Server.Storage.Abstractions;

namespace StarConflictsRevolt.Server.Storage.JsonFiles;

internal sealed class JsonFilesRepositoryProvider(JsonFilesOptions options) : IRepositoryProvider
{
    public IRepository<T> Create<T>() where T : class, IHasId => new JsonFilesRepository<T>(options);
}
