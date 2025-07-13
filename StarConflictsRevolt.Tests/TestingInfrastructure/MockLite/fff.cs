// -----------------------------------------------------------------------------
// MockLite – minimal NSubstitute-style mocking library
// © 2025 Your Name – MIT licence
// -----------------------------------------------------------------------------

namespace MockLite;

using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

/// <summary>
/// Entry point for creating substitutes.
/// </summary>
/// <example>
/// <code>
/// var calc = Substitute.For<ICalc>();
/// calc.When(c => c.Add(2, 3)).Returns(42);
/// calc.Add(2, 3);          // 42
/// calc.Received().Add(2, 3);
/// </code>
/// </example>
public static class Substitute
{
    /// <summary>Creates a substitute for <typeparamref name="T"/>.</summary>
    public static T For<T>() where T : class
        => DispatchProxy.Create<T, Proxy<T>>();
}

// ───────────────────────────── INTERNAL IMPLEMENTATION ────────────────────────
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

/// <summary>Fluent extension methods for arranging and verifying calls.</summary>
public static class SubstituteExtensions
{
    /// <summary>Starts arrangement for a <paramref name="expr"/>.</summary>
    public static Proxy<T>.RuleBuilder When<T>(this T sub,
        Expression<Action<T>> expr) where T : class => Build(sub, expr.Body);

    /// <inheritdoc cref="When{T}(T, Expression{Action{T}})"/>
    public static Proxy<T>.RuleBuilder When<T>(this T sub,
        Expression<Func<T, object?>> expr) where T : class => Build(sub, expr.Body);

    /// <summary>Verifies that the specified call was received at least once.</summary>
    public static T Received<T>(this T sub) where T : class
    {
        var verifier = DispatchProxy.Create<T, ReceivedProxy<T>>();
        ((ReceivedProxy<T>)(object)verifier).Configure(sub as Proxy<T>);
        return verifier;
    }

    // ── helpers ──
    private static Proxy<T>.RuleBuilder Build<T>(T sub, Expression body) where T : class
    {
        if (body is not MethodCallExpression call)             // Expression tree – get MethodInfo
            throw new ArgumentException("Expect a method call", nameof(body));
        return new Proxy<T>.RuleBuilder(call.Method, sub as Proxy<T>);
    }

    // nested verifier proxy
    private sealed class ReceivedProxy<T> : DispatchProxy where T : class
    {
        private Proxy<T>? _proxy;
        internal void Configure(Proxy<T>? p) => _proxy = p;

        protected override object? Invoke(MethodInfo? target, object?[]? _)
        {
            if (target is null || _proxy is null) return null;
            if (!_proxy.Calls.Any(c => c.Method == target))
                throw new Exception($"{target.Name} was not invoked.");
            return target.ReturnType.IsValueType ? Activator.CreateInstance(target.ReturnType) : null;
        }
    }
}
