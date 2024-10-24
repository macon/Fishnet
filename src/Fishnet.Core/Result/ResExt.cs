using Fishnet.Core;

namespace Fishnet.Core.Result;

public static class ResExtensions
{
    public static Res<TR, E> Select<T, E, TR>(this Res<T, E> value, Func<T, TR> f)
        => value.Map(f);

    public static Res<TR> Select<T, TR>(this Res<T> value, Func<T, TR> f)
        => value.Map(f);

    public static Res<TRR> SelectMany<T, TR, TRR>(
        this Res<T> res,
        Func<T, Res<TR>> bind,
        Func<T, TR, TRR> project)
        => res.Match(
            f => f,
            s => bind(s).Match(
                f => f,
                r => new Res<TRR>(project(s, r))));

    public static Res<T> Apply<T>(
        this Res<Func<Error, Error>> optF,
        Res<T> arg)
        => optF.Match(
            err => new Res<T>(err),
            succ => arg.Match(
                err => new Res<T>(succ(err)),
                s => s));

    public static Res<TR> Apply<T, TR>(
        this Res<Func<T, TR>> resF,
        Res<T> arg)
        => resF.Match(
            err => new Res<TR>(err),
            suc => arg.Match(
                err => new Res<TR>(err),
                s => new Res<TR>(suc(s))));

    public static Res<Func<T2, TR>> Apply<T1, T2, TR>(
        this Res<Func<T1, T2, TR>> @this,
        Res<T1> arg)
        => Apply(@this.Map(FuncExtensions.Curry), arg);

    public static Res<Func<T2, T3, TR>> Apply<T1, T2, T3, TR>(
        this Res<Func<T1, T2, T3, TR>> @this,
        Res<T1> arg)
        => Apply(@this.Map(FuncExtensions.CurryFirst), arg);

    public static Res<TR, E> Apply<T, E, TR>(
        this Res<Func<T, TR>, E> resF,
        Res<T, E> arg)
        => resF.Match(
            err => new Res<TR, E>(err),
            suc => arg.Match(
                err => new Res<TR, E>(err),
                s => new Res<TR, E>(suc(s))));

    public static Res<Func<T2, TR>, E> Apply<T1, T2, E, TR>(
        this Res<Func<T1, T2, TR>, E> @this,
        Res<T1, E> arg)
        => Apply(@this.Map(FuncExtensions.Curry), arg);

    public static Res<Func<T2, T3, TR>, E> Apply<T1, T2, T3, E, TR>(
        this Res<Func<T1, T2, T3, TR>, E> @this,
        Res<T1, E> arg)
        => Apply(@this.Map(FuncExtensions.CurryFirst), arg);

    public static Res<Func<T2, TR>, E> Map<T1, T2, E, TR>(
        this Res<T1, E> @this,
        Func<T1, T2, TR> func)
        => @this.Map(func.Curry());

    public static Res<IEnumerable<TR>> Traverse<T, TR>(
        this IEnumerable<T> ts,
        Func<T, Res<TR>> f)
        => ts.Aggregate(
            seed: new Res<IEnumerable<TR>>([]),
            func: (resRs, t) =>
                from rs in resRs
                from r in f(t)
                select rs.Append(r));

    public static Unit Match<T>(
        this Res<T> res,
        Action<Error> err,
        Action<T> succ)
        => res.Match(err.ToFunc(), succ.ToFunc());

    /// <summary>
    ///
    /// </summary>
    /// <param name="res"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static T GetOrDie<T>(this Res<T> res) where T : notnull
        => res.Match(
            _ => throw new FunctionalStateException($"{nameof(T)} is {nameof(Error)}"),
            t => t);
}
