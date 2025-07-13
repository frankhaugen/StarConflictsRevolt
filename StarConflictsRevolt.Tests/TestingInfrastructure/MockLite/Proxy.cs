using System.Collections.Concurrent;
using System.Reflection;

namespace StarConflictsRevolt.Tests.TestingInfrastructure.MockLite;

public sealed class Proxy<T> : DispatchProxy where T : class
{
    private readonly ConcurrentDictionary<MethodInfo, Func<object?[], object?>> _rules = new();
    private readonly ConcurrentQueue<Invocation> _calls = new();

    /// <inheritdoc/>
    protected override object? Invoke(MethodInfo? target, object?[]? args)
    {
        if (target is null) return null;
        args ??= Array.Empty<object?>();

        _calls.Enqueue(new(target, args));

        return _rules.TryGetValue(target, out var rule)
            ? rule(args)
            : (target.ReturnType.IsValueType ? Activator.CreateInstance(target.ReturnType) : null);
    }

    // helpers for nested types
    internal void AddRule(MethodInfo m, Func<object?[], object?> r) => _rules[m] = r;
    internal IEnumerable<Invocation> Calls => _calls;

    /// <summary>Captures one call for verification purposes.</summary>
    internal readonly record struct Invocation(MethodInfo Method, object?[] Arguments);

    /// <summary>Fluent builder returned by <see cref="SubstituteExtensions.When"/>.</summary>
    public readonly struct RuleBuilder(MethodInfo method, Proxy<T>? proxy)
    {
        /// <summary>Returns a constant <paramref name="value"/> for the arranged call.</summary>
        public void Returns(object? value)                     => proxy.AddRule(method, _ => value);
        /// <summary>Returns a computed value for the arranged call.</summary>
        public void Returns(Func<object?[], object?> factory) => proxy.AddRule(method, factory);
    }
}