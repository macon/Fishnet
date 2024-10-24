// ReSharper disable InconsistentNaming

using System.Diagnostics.CodeAnalysis;

namespace Fishnet.Core;
using static Prelude;

public readonly struct Either<L, R> : IEquatable<Either<L, R>>, IEquatable<Either.Left<L>>, IEquatable<Either.Right<R>>
{
    internal L? LeftValue { get; }
    internal R? RightValue { get; }

    [MemberNotNullWhen(true, nameof(LeftValue))]
    [MemberNotNullWhen(false, nameof(RightValue))]
    public bool IsLeft { get; }

    [MemberNotNullWhen(true, nameof(RightValue))]
    [MemberNotNullWhen(false, nameof(LeftValue))]
    public bool IsRight => !IsLeft;

    internal Either(L left) => (LeftValue, IsLeft) = (left, true);
    internal Either(R right) => (RightValue, IsLeft) = (right, false);

    public static implicit operator Either<L, R>(Either.Left<L> left) => new(left.Value);
    public static implicit operator Either<L, R>(Either.Right<R> right) => new(right.Value);

    public TR Match<TR>(Func<L, TR> left, Func<R, TR> right) where TR : notnull
        => IsLeft ? left(LeftValue!) : right(RightValue!);

    public Unit Match(Action<L> left, Action<R> right)
        => Match(left.ToFunc(), right.ToFunc());

    public static Either<L, R> Left(L left) => new(left);
    public static Either<L, R> Right(R right) => new(right);

    public Either<L, RR> Bind<RR>(Func<R, Either<L, RR>> f)
        => Match(
            l => new Either<L, RR>(l),
            f);

    public static implicit operator Either<L, R>(L value)
    {
        ArgumentNullException.ThrowIfNull(value, nameof(value));
        return Left(value);
    }
    public static implicit operator Either<L, R>(R value)
    {
        ArgumentNullException.ThrowIfNull(value, nameof(value));
        return Right(value);
    }

    public static bool operator true(Either<L, R> @this) => @this.IsRight;
    public static bool operator false(Either<L, R> @this) => @this.IsLeft;
    public static Either<L, R> operator |(Either<L, R> a, Either<L, R> b) => a.IsRight ? a : b;

    public bool Equals(Either<L, R> other)
    {
        return IsLeft == other.IsLeft
               && (IsLeft ? LeftValue!.Equals(other.LeftValue) : RightValue!.Equals(other.RightValue));
    }

    public bool Equals(Either.Left<L> other)
    {
        return !IsRight && LeftValue!.Equals(other.Value);
    }

    public bool Equals(Either.Right<R> other)
    {
        return !IsLeft && RightValue!.Equals(other.Value);
    }

    public override int GetHashCode() => IsLeft ? LeftValue!.GetHashCode() : RightValue!.GetHashCode();

    public override bool Equals(object? other) =>
        other switch
        {
            Either<L, R> either => Equals(either),
            Either.Left<L> left => Equals(left),
            Either.Right<R> right => Equals(right),
            _ => false
        };

    public override string? ToString()
    {
        return IsLeft ? LeftValue.ToString() : RightValue.ToString();
    }
}

public static class Either
{
    public readonly struct Left<L>
    {
        internal L Value { get; }
        internal Left(L value) { Value = value; }

        public override string ToString() => $"Left({Value})";
    }

    public readonly struct Right<R>
    {
        internal R Value { get; }
        internal Right(R value) { Value = value; }

        public override string ToString() => $"Right({Value})";

        public Right<RR> Map<RR>(Func<R, RR> f) => Right(f(Value));
        public Either<L, RR> Bind<L, RR>(Func<R, Either<L, RR>> f) => f(Value);
    }
}

public static class EitherExt
{
    public static Either<TL, TR> AsLeft<TL, TR>(this TL value) => Either<TL, TR>.Left(value);
    public static Either<TL, TR> AsRight<TL, TR>(this TR value) => Either<TL, TR>.Right(value);

    public static Either<L, RR> Map<L, R, RR>(
        this Either<L, R> either,
        Func<R, RR> f)
        => either.Match<Either<L, RR>>(
            l => Left(l),
            r => Right(f(r)));

    public static Either<LL, RR> Map<L, LL, R, RR>(
        this Either<L, R> either,
        Func<L, LL> left,
        Func<R, RR> right)
        =>
            either.Match<Either<LL, RR>>(
                l => Left(left(l)),
                r => Right(right(r)));

    public static Either<L, TR> Apply<L, R, TR>(
        this Either<L, Func<R, TR>> eitherF,
        Either<L, R> arg)
        => eitherF.IsRight && arg.IsRight
            ? new Either<L, TR>(eitherF.RightValue!(arg.RightValue))
            : Either<L, TR>.Left(eitherF.IsLeft ? eitherF.LeftValue! : arg.LeftValue!);
}

public static partial class Prelude
{
    public static Either.Left<L> Left<L>(L left) => new(left);
    public static Either.Right<R> Right<R>(R right) => new(right);
}
