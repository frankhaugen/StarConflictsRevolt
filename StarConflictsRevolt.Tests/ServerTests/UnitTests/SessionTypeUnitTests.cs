using StarConflictsRevolt.Server.WebApi.Models;

namespace StarConflictsRevolt.Tests.ServerTests.UnitTests;

public class SessionTypeUnitTests
{
    [Test]
    public async Task SessionType_Enum_HasExpectedValues()
    {
        await Assert.That(Enum.IsDefined(typeof(SessionType), "SinglePlayer")).IsTrue();
        await Assert.That(Enum.IsDefined(typeof(SessionType), "Multiplayer")).IsTrue();
    }

    [Test]
    public async Task Session_Model_CreatesWithCorrectType()
    {
        var session = Session.Create("Test", SessionType.SinglePlayer);
        await Assert.That(session.SessionType).IsEqualTo(SessionType.SinglePlayer);
        var session2 = Session.Create("Test2", SessionType.Multiplayer);
        await Assert.That(session2.SessionType).IsEqualTo(SessionType.Multiplayer);
    }

    [Test]
    public async Task Session_Model_Equality_WorksWithType()
    {
        var s1 = new Session(Guid.NewGuid(), "A", DateTime.UtcNow, true, null, SessionType.SinglePlayer);
        var s2 = new Session(s1.Id, "A", s1.Created, true, null, SessionType.SinglePlayer);
        await Assert.That(s1).IsEqualTo(s2);
    }

    [Test]
    public async Task SessionType_Enum_Parse_Works()
    {
        var parsed = Enum.Parse<SessionType>("SinglePlayer");
        await Assert.That(parsed).IsEqualTo(SessionType.SinglePlayer);
    }

    [Test]
    public async Task SessionType_Enum_ToString_Works()
    {
        var type = SessionType.Multiplayer;
        await Assert.That(type.ToString()).IsEqualTo("Multiplayer");
    }

    [Test]
    public async Task Session_Model_StoresAllFields()
    {
        var now = DateTime.UtcNow;
        var session = new Session(Guid.NewGuid(), "Test", now, true, now, SessionType.SinglePlayer);
        await Assert.That(session.SessionName).IsEqualTo("Test");
        await Assert.That(session.IsActive).IsTrue();
        await Assert.That(session.Ended).IsEqualTo(now);
        await Assert.That(session.SessionType).IsEqualTo(SessionType.SinglePlayer);
    }

    [Test]
    public async Task SessionType_Enum_Invalid_Throws()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Task.FromResult(Enum.Parse<SessionType>("InvalidType")));
    }

    [Test]
    public async Task SessionType_Enum_Values_AreUnique()
    {
        var values = Enum.GetValues<SessionType>();
        await Assert.That(values).HasCount(2);
        await Assert.That(values).Contains(SessionType.SinglePlayer);
        await Assert.That(values).Contains(SessionType.Multiplayer);
    }
}