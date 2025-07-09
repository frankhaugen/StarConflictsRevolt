using StarConflictsRevolt.Tests.ExampleTests.Data;

namespace StarConflictsRevolt.Tests.ExampleTests;

[ClassDataSource<DataClass>]
[ClassConstructor<DependencyInjectionClassConstructor>]
public class AndEvenMoreTests(DataClass dataClass)
{
    [Test]
    public void HaveFun()
    {
        Console.WriteLine(dataClass);
        Console.WriteLine("For more information, check out the documentation");
        Console.WriteLine("https://tunit.dev/");
    }
}