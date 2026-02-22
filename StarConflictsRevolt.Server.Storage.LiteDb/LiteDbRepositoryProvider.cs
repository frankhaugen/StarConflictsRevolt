using LiteDB;
using StarConflictsRevolt.Server.Storage.Abstractions;

namespace StarConflictsRevolt.Server.Storage.LiteDb;

internal sealed class LiteDbRepositoryProvider(ILiteDatabase db) : IRepositoryProvider
{
    public IRepository<T> Create<T>() where T : class, IHasId => new LiteDbRepository<T>(db);
}
