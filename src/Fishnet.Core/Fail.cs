namespace Fishnet.Core;
using static Prelude;

public record Fail
{
    private Fail<Error> Value { get; }

    public Fail(Error error) => Value = new Fail<Error>(error);

    public Fail(Exception exception) => Value = new Fail<Error>(exception);

    public static implicit operator Fail(Error error) => new(error);
    public static implicit operator Fail(Exception ex) => new(ex);

    public TR Match<TR>(Func<Exception, TR> exception, Func<Error, TR> error) where TR : notnull
        => Value.Match(exception, error);

    public Fail Bind(Func<Error, Fail> error)
    {
        return Value.Match(
            ex => new Fail(ex),
            error);
    }

    public Fail Map(Func<Error, Error> error)
        => Match(
            ex => new Fail(ex),
            err => new Fail(error(err)));
}

public record Fail<T>
{
    private Either<Exception, T> Value { get; }

    public Fail(T error) => Value = Right(error);

    public Fail(Exception exception) => Value = Left(exception);

    public Fail(Either<Exception, T> fail) => Value = fail;

    public static implicit operator Fail<T>(T message) => new(message);
    public static implicit operator Fail<T>(Exception ex) => new(ex);
    public static implicit operator Fail<T>(Either<Exception, T> fail) => new(fail);

    public TR Match<TR>(Func<Exception, TR> exception, Func<T, TR> error) where TR : notnull
        => Value.Match(exception, error);

    public Fail<TR> Bind<TR>(Func<T, Fail<TR>> error)
    {
        return Value.Match(
            l => new Fail<TR>(l),
            error);
    }

    public Fail<TR> Map<TR>(Func<T, TR> error) where TR : notnull
    {
        var x = Value.Map(error);

        return new Fail<TR>(x);
    }

    public override string ToString()
        => Match(
            ex => ex.ToString(),
            err => err?.ToString() ?? "Error");

    public bool IsError => Value.IsRight;
    public bool IsException => Value.IsLeft;
}

public static partial class Prelude
{
    public static Fail<T> Error<T>(T error) => new(error);
    public static Fail<T> Exception<T>(Exception ex) => new(ex);
    public static Fail<Error> Exception(Exception ex) => new(ex);
}
