// -----------------------------------------------------------------------------
// MockLite – minimal NSubstitute-style mocking library
// © 2025 Your Name – MIT licence
// -----------------------------------------------------------------------------

using System.Reflection;

namespace StarConflictsRevolt.Tests.TestingInfrastructure.MockLite;

/// <summary>
///     Entry point for creating substitutes.
/// </summary>
/// <example>
///     <code>
/// var calc = Substitute.For<ICalc>
///             ();
///             calc.When(c => c.Add(2, 3)).Returns(42);
///             calc.Add(2, 3);          // 42
///             calc.Received().Add(2, 3);
/// </code>
/// </example>
public static class Substitute
{
    /// <summary>Creates a substitute for <typeparamref name="T" />.</summary>
    public static T For<T>() where T : class
    {
        return DispatchProxy.Create<T, Proxy<T>>();
    }
}