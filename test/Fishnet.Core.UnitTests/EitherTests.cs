using FluentAssertions;

namespace Fishnet.Core.UnitTests;

public class EitherTests
{
    [Fact]
    public void LeftTests()
    {
        Either<string, int>.Left("bang").IsLeft
            .Should().BeTrue();

        "bang".AsLeft<string, int>().IsLeft
            .Should().BeTrue();

        1.AsRight<string, int>().IsRight
            .Should().BeTrue();
    }

    [Fact]
    public void MatchTests()
    {
        Either<string, int>.Right(12)
            .Match(
                _ => 0,
                r => r * 2)
            .Should().Be(24);

        Either<string, int>.Left("Boom!")
            .Match(
                _ => 0,
                r => r * 2)
            .Should().Be(0);

        var signal = 0;
        Either<string, int>.Left("Boom!")
            .Match(
                _ => signal = 0,
                r => signal = 1);
        signal.Should().Be(0);

        Either<string, int>.Right(12)
            .Match(
                left: _ => signal = 0,
                right: _ => signal = 1);
        signal.Should().Be(1);
    }

    [Fact]
    public void MapTests()
    {
        Either<string, int>.Right(12)
            .Map(r => r * 2)
            .Should().Be(Right(24));

        Either<string, int>.Left("Boom!")
            .Map(r => r * 2)
            .Should().Be(Left("Boom!"));

        Either<string, int>.Left("Boom!")
            .Map(
                l => $"Error: {l}",
                r => r * 2)
            .Should().Be(Left("Error: Boom!"));

        Either<string, int>.Right(2)
            .Map(
                l => $"Error: {l}",
                r => r * 2)
            .Should().Be(Right(4));
    }
}
