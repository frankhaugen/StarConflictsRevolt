using TUnit.Core;

namespace StarConflictsRevolt.Tests;

public class SimpleTest
{
    [Test]
    public async Task Simple_Test_Should_Pass()
    {
        await Assert.That(1 + 1).IsEqualTo(2);
    }
    
    [Test]
    public async Task Simple_Async_Test_Should_Pass()
    {
        await Task.Delay(100); // Small delay to test async
        await Assert.That(true).IsTrue();
    }
} 