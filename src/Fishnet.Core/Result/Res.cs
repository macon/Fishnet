// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

using Fishnet.Core;

namespace Fishnet.Core;

public readonly struct Res<T> : IEquatable<Res<T>>
{
    private Res<T, Error> ThisRes { get; }

    public Res(T value) => ThisRes = new Res<T, Error>(value);
    public Res(Error error) => ThisRes = new Res<T, Error>(error);

    public bool IsError => ThisRes.IsError;
    public bool IsSuccess => ThisRes.IsSuccess;

    public static implicit operator Res<T>(T right) => new(right);
    public static implicit operator Res<T>(Error left) => new(left);

    public bool Equals(Res<T> other) =>
        IsSuccess == other.IsSuccess
        && (IsError || ThisRes.Equals(other.ThisRes));

    public override bool Equals(object? other) =>
        other switch
        {
            Res<T> resOther => Equals(resOther),
            _ => false
        };

    public override int GetHashCode() => ThisRes.GetHashCode();

    public TR Match<TR>(Func<Error, TR> error, Func<T, TR> suc) where TR : notnull
        => ThisRes.Match(
            error,
            suc);

    public Res<TR> Map<TR>(Func<T, TR> success)
        => Match(
            fail => new Res<TR>(fail),
            suc => new Res<TR>(success(suc)));

    public Res<TR> Bind<TR>(Func<T, Res<TR>> success) =>
        Match(
            err => new Res<TR>(err),
            success);

    public static bool operator ==(Res<T> left, Res<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Res<T> left, Res<T> right)
    {
        return !(left == right);
    }
}

public static partial class Prelude
{
    public static Res<T> Success<T>(T value) => new(value);
    public static Res<T> Error<T>(Error err) => new(err);
    public static Res<T> Error<T>(string err) => new(err);
}
