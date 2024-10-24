namespace Fishnet.Core;

public static class FuncExtensions
{
    public static Func<T1, Func<T2, TR>> Curry<T1, T2, TR>(this Func<T1, T2, TR> f)
        => t1 => t2 => f(t1, t2);

    public static Func<T1, Func<T2, Func<T3, TR>>> Curry<T1, T2, T3, TR>(this Func<T1, T2, T3, TR> f)
        => t1 => t2 => t3 => f(t1, t2, t3);

    public static Func<T1, Func<T2, T3, TR>> CurryFirst<T1, T2, T3, TR>(this Func<T1, T2, T3, TR> @this)
        => t1 => (t2, t3) => @this(t1, t2, t3);

    public static Func<T1, Func<T2, T3, T4, TR>> CurryFirst<T1, T2, T3, T4, TR>
        (this Func<T1, T2, T3, T4, TR> @this) => t1 => (t2, t3, t4) => @this(t1, t2, t3, t4);
}
