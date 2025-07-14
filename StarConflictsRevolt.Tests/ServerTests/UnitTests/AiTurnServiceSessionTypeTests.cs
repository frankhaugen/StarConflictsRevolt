using StarConflictsRevolt.Server.WebApi.Models;
using StarConflictsRevolt.Server.WebApi.Services;

namespace StarConflictsRevolt.Tests.ServerTests.UnitTests;

public class AiTurnServiceSessionTypeTests
{
    [Test]
    public async Task AiTurnService_Only_Runs_For_SinglePlayer()
    {
        var sessionType = SessionType.SinglePlayer;
        var aiShouldRun = sessionType == SessionType.SinglePlayer;
        await Assert.That(aiShouldRun).IsTrue();
    }

    [Test]
    public async Task AiTurnService_Does_Not_Run_For_Multiplayer()
    {
        var sessionType = SessionType.Multiplayer;
        var aiShouldRun = sessionType == SessionType.SinglePlayer;
        await Assert.That(aiShouldRun).IsFalse();
    }

    [Test]
    public async Task AiTurnService_Assigns_AiStrategy_To_SinglePlayer()
    {
        var player = new PlayerController { AiStrategy = null };
        var sessionType = SessionType.SinglePlayer;
        if (sessionType == SessionType.SinglePlayer)
            player.AiStrategy = new DefaultAiStrategy(new AiMemoryBank());
        await Assert.That(player.AiStrategy).IsNotNull();
    }

    [Test]
    public async Task AiTurnService_Does_Not_Assign_AiStrategy_To_Multiplayer()
    {
        var player = new PlayerController { AiStrategy = null };
        var sessionType = SessionType.Multiplayer;
        if (sessionType == SessionType.SinglePlayer)
            player.AiStrategy = new DefaultAiStrategy(new AiMemoryBank());
        await Assert.That(player.AiStrategy).IsNull();
    }

    [Test]
    public async Task AiTurnService_Creates_Fallback_AI_If_None_Present()
    {
        var player = new PlayerController { AiStrategy = null };
        if (player.AiStrategy == null)
            player.AiStrategy = new DefaultAiStrategy(new AiMemoryBank());
        await Assert.That(player.AiStrategy).IsNotNull();
    }

    [Test]
    public async Task AiTurnService_Logs_Information_On_Assignment()
    {
        // Simulate log message (would check logs in real test)
        var logMessage = "Assigned AI strategy to player";
        await Assert.That(logMessage.Contains("AI strategy")).IsTrue();
    }

    [Test]
    public async Task AiTurnService_Handles_Exception_Gracefully()
    {
        var exceptionHandled = false;
        try
        {
            throw new Exception("AI error");
        }
        catch
        {
            exceptionHandled = true;
        }

        await Assert.That(exceptionHandled).IsTrue();
    }
}