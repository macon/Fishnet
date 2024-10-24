using FluentAssertions;
using Fishnet.Core.Result;
using Fishnet.Core;

namespace Fishnet.Core.UnitTests.ResultTests;

internal enum ErrorType
{
    Bad = 1,
    Worse = 2,
    Terrible = 3
}

internal record MyError(ErrorType ErrorType, string Message)
{
    public override string ToString() => $"{ErrorType}: {Message}";
}

// ReSharper disable once InconsistentNaming
public class ResOfETests // Res<T,E> Tests
{
    [Fact]
    public void SuccessTests()
    {
        new Res<string, MyError>("yay").IsSuccess
            .Should().BeTrue();
    }

    [Fact]
    public void MatchTests()
    {
        new Res<string, MyError>("yay")
            .Match(
                e => e.Message,
                s => s)
            .Should().Be("yay");

        new Res<string, MyError>(new MyError(ErrorType.Terrible, "Boom!"))
            .Match(
                e => e.Message,
                s => s)
            .Should().Be("Boom!");
    }

    [Fact]
    public void MapTests()
    {
        new Res<string, MyError>("yay")
            .Map(s => s.Length)
            .Should().Be(new Res<int, MyError>(3));

        new Res<string, MyError>(new MyError(ErrorType.Bad, "Boom!"))
            .Map(s => s.Length)
            .Should().Be(new Res<int, MyError>(new MyError(ErrorType.Bad, "Boom!")));
    }

    [Fact]
    public void BindTests()
    {
        new Res<string, MyError>("yay")
            .Bind(s => new Res<int, MyError>(s.Length))
            .Should().Be(new Res<int, MyError>(3));

        new Res<string, MyError>(new MyError(ErrorType.Worse, "Boom!"))
            .Bind(s => new Res<int, MyError>(s.Length))
            .Should().Be(new Res<int, MyError>(new MyError(ErrorType.Worse, "Boom!")));
    }

    [Fact]
    public void QueryTests()
    {
        var x = new Res<string, MyError>("yay");

        var y = from z in x
                select z.Length;

        y.Should().Be(new Res<int, MyError>(3));
    }

    [Fact]
    public void ErrorTests()
    {
        new Res<string, MyError>(new MyError(ErrorType.Worse, "Boom!"))
            .IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void ApplyTests()
    {
        var mult = (int x, int y) => x * y;

        var two = new Res<int, MyError>(2);
        var three = new Res<int, MyError>(3);


        var y = two
            .Map(mult)
            .Apply(three);

        y.Should().Be(new Res<int, MyError>(6));
    }
}
