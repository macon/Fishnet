using System.Diagnostics.CodeAnalysis;

// ReSharper disable InconsistentNaming

namespace Fishnet.Core;

public readonly struct Exc<T> : IEquatable<Exc<T>>
{
    private Exception? Ex { get; }
    private T? Value { get; }

    public Exc(T value) => (Value, IsSuccess) = (value, true);
    public Exc(Exception ex) => (Ex, IsSuccess) = (ex, false);

    [MemberNotNullWhen(true, nameof(Ex))]
    [MemberNotNullWhen(false, nameof(Value))]
    public bool IsException => !IsSuccess;

    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Ex))]
    public bool IsSuccess { get; }

    public string State => ToString();

    public static implicit operator Exc<T>(Exception ex) => new(ex);
    public static implicit operator Exc<T>(T t) => new(t);
    public static bool operator ==(Exc<T> left, Exc<T> right) => Equals(left, right);
    public static bool operator !=(Exc<T> left, Exc<T> right) => !(left == right);

    public TR Match<TR>(Func<T, TR> success, Func<Exception, TR> ex) where TR : notnull
        => IsSuccess ? success(Value) : ex(Ex);

    public bool Equals(Exc<T> other)
        => IsSuccess == other.IsSuccess
           && (IsException || Value.Equals(other.Value));

    public override bool Equals(object? other)
        => other switch
        {
            Exc<T> excOther => Equals(excOther),
            _ => false
        };

    public override int GetHashCode()
        => Match(
            t => t!.GetHashCode(),
            e => e.GetHashCode());

    public override string ToString()
    {
        return Match<string>(
            ex: e => $"Exception: {e.Message}",
            success: s => $"Success: {s}");
    }

    public Exc<TR> Map<TR>(Func<T, TR> success)
        => Match(
            suc => new Exc<TR>(success(suc)),
            ex => new Exc<TR>(ex));

    public Exc<T> Do(Action<T> success)
    {
        if (IsSuccess)
        {
            success(Value);
        }
        return this;
    }
}

public static class ExcExt
{
    public static Unit Match<T>(
        this Exc<T> exc,
        Action<Exception> ex,
        Action<T> success)
        => exc.Match(success.ToFunc(), ex.ToFunc());

    public static Exc<T> OrElse<T>(this Exc<T> left, Exc<T> right) => left.IsSuccess ? left : right;

    public static Exc<R> AndThen<T, R>(this Exc<T> left, Func<T, Exc<R>> func)
        => left.Bind(func);

    public static Exc<R> Bind<T, R>(this Exc<T> left, Func<T, Exc<R>> func)
        => left.Match(
            success: func,
            ex => new Exc<R>(ex));
}

public static partial class Prelude
{
    public static Exc<T> Ok<T>(T value) => new(value);
    public static Exc<T> Exc<T>(Exception ex) => new(ex);
}
