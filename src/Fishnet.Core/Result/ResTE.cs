// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

using System.Diagnostics.CodeAnalysis;

namespace Fishnet.Core;

public readonly struct Res<T, E> : IEquatable<Res<T, E>>
{
    private E? Error { get; }
    private T? Value { get; }

    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }

    [MemberNotNullWhen(true, nameof(Error))]
    [MemberNotNullWhen(false, nameof(Value))]
    public bool IsError => !IsSuccess;

    public Res(T value) => (Value, Error, IsSuccess) = (value, default, true);
    public Res(E error) => (Value, Error, IsSuccess) = (default, error, false);

    public static implicit operator Res<T, E>(T value) => new(value);
    public static implicit operator Res<T, E>(E error) => new(error);
    public static bool operator ==(Res<T, E> left, Res<T, E> right) => Equals(left, right);
    public static bool operator !=(Res<T, E> left, Res<T, E> right) => !(left == right);
    public static bool operator true(Res<T, E> @this) => @this.IsSuccess;
    public static bool operator false(Res<T, E> @this) => @this.IsError;
    public static Res<T, E> operator |(Res<T, E> left, Res<T, E> right) => left.IsSuccess ? left : right;
    public static implicit operator string(Res<T, E> res) => res.ToString();

    public bool Equals(Res<T, E> other)
        => IsSuccess == other.IsSuccess && (IsError || Value.Equals(other.Value));

    public override bool Equals(object? other)
        => other switch
        {
            Res<T, E> resOther => Equals(resOther),
            _ => false
        };

    public override string ToString()
        => Match(
            e => $"Error: {e}",
            t => $"Success: {t}");

    public override int GetHashCode() => IsSuccess ? Value.GetHashCode() : Error.GetHashCode();

    public TR Match<TR>(Func<E, TR> error, Func<T, TR> success) where TR : notnull
        => IsSuccess ? success(Value) : error(Error);

    public Res<TR, E> Map<TR>(Func<T, TR> success)
        => Match(
            err => new Res<TR, E>(err),
            suc => new Res<TR, E>(success(suc)));

    public Res<TR, E> Bind<TR>(Func<T, Res<TR, E>> success)
        => Match(
            err => new Res<TR, E>(err),
            success: success);
}
