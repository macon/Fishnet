// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

using Fishnet.Core;

namespace Fishnet.Core;

public delegate Exc<T> Try<T>();

public static class TryExt
{
    public static Exc<T> Run<T>(this Try<T> func)
    {
        try { return func(); }
        catch (Exception ex) { return ex; }
    }
}

public static partial class Prelude
{
    public static Try<T> Try<T>(Func<T> func) => () => Ok(func());
}
