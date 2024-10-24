// ReSharper disable InconsistentNaming

namespace Fishnet.Core;

public static class EnumerableExt
{
    public static R Match<T, R>(
        this IEnumerable<T> list,
        Func<R> empty,
        Func<T, IEnumerable<T>, R> otherwise) where T : notnull
    {
        var enumerable = list.ToList();
        return enumerable.Head().Match(
            none: empty,
            some: head => otherwise(head, enumerable.Skip(1)));
    }

    private static Opt<T> Head<T>(this IEnumerable<T> list) where T : notnull
    {
        if (list == null) { return None; }
        using var enumerator = list.GetEnumerator();
        return enumerator.MoveNext() ? Some(enumerator.Current) : None;
    }
}
