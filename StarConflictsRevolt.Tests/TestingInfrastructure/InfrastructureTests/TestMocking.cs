using StarConflictsRevolt.Tests.TestingInfrastructure.MockLite;

namespace StarConflictsRevolt.Tests.TestingInfrastructure.InfrastructureTests;

public partial class TestMocking
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

    public interface ITestInterface
    {
        int Add(int a, int b);
    }
}