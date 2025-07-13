using StarConflictsRevolt.Tests.TestingInfrastructure.MockLite;

namespace StarConflictsRevolt.Tests.TestingInfrastructure.InfrastructureTests;

public class TestMocking
{
    [Test]
    public async Task Substitute_CanCreateMock()
    {
        var mock = Substitute.For<ITestInterface>();

        await Assert.That(mock).IsNotNull();
        await Assert.That(mock.Add(1, 2)).IsEqualTo(0); // Default return value for int is 0
    }

    [Test]
    public async Task Substitute_CanSetupAndVerifyMethodCall()
    {
        var mock = Substitute.For<ITestInterface>();
        mock.When(m => m.Add(2, 3)).Returns(42);

        var result = mock.Add(2, 3);

        await Assert.That(result).IsEqualTo(42);
    }

    [Test]
    public async Task Substitute_BasicMockingWorks()
    {
        var mock = Substitute.For<ITestInterface>();

        // Test that the mock is created and returns default values
        var result = mock.Add(5, 10);
        await Assert.That(result).IsEqualTo(0); // Default for int
    }

    public interface ITestInterface
    {
        int Add(int a, int b);
    }
}