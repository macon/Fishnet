// ReSharper disable CheckNamespace

using Fishnet.Core;

namespace Fishnet.Core;

public static class OptExt
{
    public static Opt<T> Of<T>(this NoneType _) where T : notnull => Opt<T>.None;

    /// <summary>
    /// Applies the function to the value if it is Some, otherwise returns None.
    /// </summary>
    /// <remarks>
    /// M&lt;T&gt; -&gt; (T -&gt; R) -&amp;gt; M&lt;R&gt;
    /// </remarks>
    public static Opt<R> Map<T, R>(
        this Opt<T> opt,
        Func<T, R> f)
        where R : notnull
        where T : notnull
        =>
            opt.Match(() => None, t => Some(f(t)));

    public static Opt<Func<T2, R>> Map<T1, T2, R>(
        this Opt<T1> opt,
        Func<T1, T2, R> f)
        where T1 : notnull
        =>
            opt.Map(f.Curry());

    public static Opt<Func<T2, T3, R>> Map<T1, T2, T3, R>(
        this Opt<T1> opt,
        Func<T1, T2, T3, R> f)
        where T1 : notnull
        =>
            opt.Map(f.CurryFirst());

    public static Opt<R> Apply<T, R>(
        this Opt<Func<T, R>> optF,
        Opt<T> arg)
        where R : notnull
        where T : notnull
        =>
            optF.Match(
                none: () => None,
                some: func => arg.Match(
                    none: () => None,
                    some: t => Some(func(t))));

    public static Opt<Func<T2, R>> Apply<T1, T2, R>(
        this Opt<Func<T1, T2, R>> optF,
        Opt<T1> optT)
        where T1 : notnull
        =>
            Apply(optF.Map(FuncExtensions.Curry), optT);

    public static Opt<Func<T2, Func<T3, R>>> Apply<T1, T2, T3, R>(
        this Opt<Func<T1, T2, T3, R>> optF,
        Opt<T1> optT)
        where T1 : notnull
        =>
            Apply(optF.Map(FuncExtensions.Curry), optT);

    public static Opt<R> Bind<T, R>(
        this Opt<T> opt,
        Func<T, Opt<R>> f)
        where R : notnull
        where T : notnull
        =>
            opt.Match(
                () => None,
                f);

    public static Opt<R> Select<T, R>(
        this Opt<T> opt,
        Func<T, R> f)
        where R : notnull
        where T : notnull
        =>
            opt.Map(f);

    public static Opt<R> SelectMany<T, TR, R>(this Opt<T> opt,
        Func<T, Opt<TR>> bind,
        Func<T, TR, R> project)
        where R : notnull
        where T : notnull
        where TR : notnull
        =>
            opt.Match(
                () => None,
                s => bind(s)
                    .Match(
                        () => None,
                        r => Some(project(s, r))));

    public static Opt<T> Do<T>(
        this Opt<T> opt,
        Action<T> some)
        where T : notnull
    {
        opt.Map(s => some.ToFunc()(s));
        return opt;
    }

    public static Unit Match<T>(
        this Opt<T> opt,
        Action none,
        Action<T> some)
        where T : notnull
        =>
            opt.Match(none.ToFunc(), some.ToFunc());

    public static Opt<IEnumerable<R>> Traverse<T, R>(this IEnumerable<T> ts,
        Func<T, Opt<R>> f)
        where R : notnull
        =>
            ts.Aggregate(
                seed: Some(Enumerable.Empty<R>()),
                func: (optRs, t) =>
                    from rs in optRs
                    from r in f(t)
                    select rs.Append(r));

    public static T GetOrElse<T>(
        this Opt<T> opt, T @default)
        where T : notnull
        =>
            opt.Match(
                () => @default,
                t => t);

    public static T GetOrElse<T>(
        this Opt<T> opt,
        Func<T> @else)
        where T : notnull
        =>
            opt.Match(
                @else,
                t => t);

    public static Opt<T> OrElse<T>(
        this Opt<T> left,
        Opt<T> right)
        where T : notnull
        =>
            left.IsSome ? left : right;

    public static T GetOrDie<T>(this Opt<T> opt)
        where T : notnull
        =>
            opt.Match(
                () => throw new FunctionalStateException($"{nameof(T)} is None"),
                t => t);

    public static Opt<R> AndThen<T, R>(
        this Opt<T> left,
        Func<T, Opt<R>> func)
        where R : notnull
        where T : notnull
        =>
            left.Bind(func);
}
