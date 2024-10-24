// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

using Fishnet.Core;

namespace Fishnet.Core;
using static Fishnet.Core.Prelude;

public readonly record struct OneTest<T1, T2> where T1 : notnull where T2 : notnull
{
    public Opt<T1> Opt1 { get; }
    public Opt<T2> Opt2 { get; }
    internal Variant Index { get; }
    internal enum Variant { T1, T2 }

    internal T1 ValT1 => Opt1.GetOrDie();
    internal T2 ValT2 => Opt2.GetOrDie();

    /// <summary>Used in switch expressions. Match against Some&lt;T&gt; to get the value.</summary>
    public ISome Some => Index switch
    {
        Variant.T1 => new Some<T1>(ValT1),
        Variant.T2 => new Some<T2>(ValT2),
        _ => throw new FunctionalStateException($"{nameof(Index)} out of range")
    };

    public bool IsT1 => Index == Variant.T1;
    public bool IsT2 => Index == Variant.T2;

    public void Deconstruct(out Opt<T1> t1, out Opt<T2> t2)
        => (t1, t2) = (Opt1, Opt2);

    public OneTest(T1 value) => (Opt1, Index) = (value, Variant.T1);
    public OneTest(T2 value) => (Opt2, Index) = (value, Variant.T2);

    public static implicit operator OneTest<T1, T2>(T1 value) => new(value);
    public static implicit operator OneTest<T1, T2>(T2 value) => new(value);
}

public static partial class OneExt
{
    // public static Unit Match<T1, T2, TR>(
    //     this One<T1, T2, TR> one,
    //     Action none,
    //     Action<T> some)
    //     => @this.Match(none.ToFunc(), some.ToFunc());

    public static TR Match<T1, T2, TR>(
        this OneTest<T1, T2> one,
        Func<T1, TR> t1Func,
        Func<T2, TR> t2Func
    ) where T1 : notnull where T2 : notnull =>
        one switch
        {
            { Index: One<T1, T2>.Variant.T1 } => t1Func(one.ValT1),
            { Index: One<T1, T2>.Variant.T2 } => t2Func(one.ValT2),
            _ => throw new FunctionalStateException($"{nameof(Index)} out of range")
        };

    /// <summary>
    /// Projects a One&lt;T1, T2&gt; to a One&lt;R1, R2&gt; using the provided mapping functions.
    /// </summary>
    public static One<T1R, T2R> Map<T1, T2, T1R, T2R>(
        this OneTest<T1, T2> one,
        Func<T1, T1R> t1Func,
        Func<T2, T2R> t2Func
    ) where T1 : notnull where T2 : notnull =>
        one.Match(
            t1 => new One<T1R, T2R>(t1Func(t1)),
            t2 => new One<T1R, T2R>(t2Func(t2)));

    /// <summary>
    /// Possibly projects a One&lt;T1, T2&gt; to an Opt&lt;One&lt;R1, R2&gt;&gt; using the optionally provided mapping functions.
    /// </summary>
    public static Opt<One<R1, R2>> Map<T1, T2, R1, R2>(
        this OneTest<T1, T2> one,
        Opt<Func<T1, R1>> t1Func,
        Opt<Func<T2, R2>> t2Func)
        where T1 : notnull
        where T2 : notnull
        where R1 : notnull
        where R2 : notnull
        =>
            one.Match<T1, T2, Opt<One<R1, R2>>>(
                t1 => t1Func
                    .Map(f => f(t1))
                    .Match(
                        none: () => Opt<One<R1, R2>>.None,
                        some: s => Some(new One<R1, R2>(s))),

                t2 => t2Func
                    .Map(f => f(t2))
                    .Match(
                        none: () => Opt<One<R1, R2>>.None,
                        some: s => Some(new One<R1, R2>(s))));

    /// <summary>
    /// Possibly projects a <c>One&lt;T1, T2&gt;</c> to an <c>Opt&lt;One&lt;R1, T2&gt;&gt;</c> using the T1->R1 mapping function.
    /// </summary>
    /// <returns><c>None</c> if <c>one != T1</c></returns>
    public static Opt<One<R1, T2>> Map<T1, T2, R1>(
        this OneTest<T1, T2> one,
        Func<T1, R1> t1Func
    ) where T1 : notnull where T2 : notnull =>
        one.Match<T1, T2, Opt<One<R1, T2>>>(
            t1 => Some(new One<R1, T2>(t1Func(t1))),
            _ => Opt<One<R1, T2>>.None);

    /// <summary>
    /// Possibly projects a <c>One&lt;T1, T2&gt;</c> to an <c>Opt&lt;One&lt;T1, R2&gt;&gt;</c> using the T2->R2 mapping function.
    /// </summary>
    /// <returns><c>None</c> if <c>one != T2</c></returns>
    public static Opt<One<T1, R2>> Map<T1, T2, R2>(
        this OneTest<T1, T2> one,
        Func<T2, R2> t2Func
    ) where T1 : notnull where T2 : notnull =>
        one.Match<T1, T2, Opt<One<T1, R2>>>(
            _ => Opt<One<T1, R2>>.None,
            t2 => Some(new One<T1, R2>(t2Func(t2))));
}
