using System;
using System.Linq;
using System.Threading.Tasks;
using StarConflictsRevolt.Tests.TestingInfrastructure;
using TUnit;

namespace StarConflictsRevolt.Tests.TestingInfrastructure.InfrastructureTests;

public class InMemoryDocumentStoreTests
{
    [Test]
    public async Task Store_And_Query_Object_Works()
    {
        var store = new InMemoryDocumentStore();
        var entity = new TestEntity { Id = "TestEntity/1", Name = "Test" };
        await using var session = store.OpenAsyncSession();
        await session.StoreAsync(entity, entity.Id);
        await session.SaveChangesAsync();

        var results = session.Query<TestEntity>().ToList();
        await Assert.That(results).HasCount(1);
        await Assert.That(results[0].Name).IsEqualTo("Test");
    }

    [Test]
    public async Task Delete_Object_Removes_From_Store()
    {
        var store = new InMemoryDocumentStore();
        var entity = new TestEntity { Id = "TestEntity/2", Name = "ToDelete" };
        await using var session = store.OpenAsyncSession();
        await session.StoreAsync(entity, entity.Id);
        await session.SaveChangesAsync();
        await session.DeleteAsync(entity.Id);
        await session.SaveChangesAsync();
        var results = session.Query<TestEntity>().ToList();
        await Assert.That(results).IsEmpty();
    }

    [Test]
    public async Task Store_Events_Are_Fired()
    {
        var store = new InMemoryDocumentStore();
        bool sessionCreated = false;
        store.OnSessionCreated += (_, _) => sessionCreated = true;
        await using var session = store.OpenAsyncSession();
        bool beforeStore = false, afterSave = false;
        session.OnBeforeStore += (_, args) => beforeStore = args.Entity is TestEntity;
        session.OnAfterSaveChanges += (_, _) => afterSave = true;
        var entity = new TestEntity { Id = "TestEntity/3", Name = "Evented" };
        await session.StoreAsync(entity, entity.Id);
        await session.SaveChangesAsync();
        await Assert.That(sessionCreated).IsTrue();
        await Assert.That(beforeStore).IsTrue();
        await Assert.That(afterSave).IsTrue();
    }

    private class TestEntity
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
} 