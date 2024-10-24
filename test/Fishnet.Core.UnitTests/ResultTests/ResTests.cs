using FluentAssertions;
using Fishnet.Core.Result;
using Fishnet.Core;

namespace Fishnet.Core.UnitTests.ResultTests;

public class ResTests
{
    [Fact]
    public void SuccessTests()
    {
        new Res<string>("yay").IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void MatchTests()
    {
        new Res<string>("yay")
            .Match(
                e => e.Message,
                s => s)
            .Should().Be("yay");

        new Res<string>(Error.New("Boom!"))
            .Match(
                e => e.Message,
                s => s)
            .Should().Be("Boom!");
    }

    [Fact]
    public void MapTests()
    {
        new Res<string>("yay")
            .Map(s => s.Length)
            .Should().Be(new Res<int>(3));

        var x = new Res<string>(Error.New("Boom!"))
            .Map(s => s.Length);

        new Res<string>(Error.New("Boom!"))
            .Map(s => s.Length)
            .Should().Be(Error<int>("Boom!"));
    }

    [Fact]
    public void BindTests()
    {
        new Res<string>("yay")
            .Bind(s => new Res<int>(s.Length))
            .Should().Be(new Res<int>(3));

        new Res<string>(Error.New("Boom!"))
            .Bind(s => new Res<int>(s.Length))
            .Should().Be(new Res<int>(Error.New("Boom!")));
    }

    [Fact]
    public void QueryTests()
    {
        var x = new Res<string>("yay");

        var y = from z in x
                select z.Length;

        y.Should().Be(new Res<int>(3));
    }

    [Fact]
    public void ErrorTests()
    {
        new Res<string>(Error.New("Boom!")).IsSuccess
            .Should().BeFalse();
    }

    [Fact]
    public void ApplyTests()
    {
        new Res<string>(Error.New("Boom!")).IsSuccess
            .Should().BeFalse();
    }

    [Theory]
    [InlineData("1, 2,  7", 10)]
    [InlineData("1, 2,  XX", null)]
    public void TraverseTests(string nums, int? result)
    {
        var expectedResult = result is null ? Error<int>("Could not parse") : Success(result.Value);
        Res<int> IntParse(string s) => int.TryParse(s, out var i) ? Success(i) : Error<int>("Could not parse");

        // Act
        var sumOpt =
            nums.Split(',')                // array of strings
                .Select(s => s.Trim())     // IEnumerable<string>
                .Traverse(IntParse)        // Opt<IEnumerable<int>>
                .Map(Enumerable.Sum);      // Opt<int>

        sumOpt.Should().Be(expectedResult);
    }
}
