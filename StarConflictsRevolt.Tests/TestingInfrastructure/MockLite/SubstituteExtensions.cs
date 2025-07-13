using System.Linq.Expressions;
using System.Reflection;

namespace MockLite;

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