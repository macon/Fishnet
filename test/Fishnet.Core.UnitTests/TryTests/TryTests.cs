using FluentAssertions;
using Fishnet.Core;

namespace Fishnet.Core.UnitTests.TryTests;

public class TryTests
{
    private static Try<int> TryDivide(int a, int b) => () => a / b;

    private readonly Try<int> _badOp = TryDivide(10, 0);
    private readonly Try<int> _goodOp = TryDivide(10, 2);

    [Fact]
    public void WhenCalledDirectThrows()
    {
        Action act = () => _badOp();
        act.Should().Throw<DivideByZeroException>();
    }

    [Fact]
    public void TryRunHandlesException()
    {
        _badOp.Run().IsException
            .Should().BeTrue();

        _badOp.Run().Should().BeOfType<Exc<int>>();

        _badOp.Run().Match<string>(
                success: i => i.ToString(),
                ex: e => e.Message)
            .Should().Be("Attempted to divide by zero.");
    }

    [Fact]
    public void TryRunSucceeds()
    {
        _goodOp.Run().IsException
            .Should().BeFalse();

        _goodOp.Run().Should().BeOfType<Exc<int>>();

        _goodOp.Run().Match<string>(
                success: i => i.ToString(),
                ex: e => e.Message)
            .Should().Be("5");
    }

    [Fact]
    public void Try_ExtensionMethod()
    {
        var goodServiceCall = FlakyServiceCall(9);

        goodServiceCall.Run().IsException
            .Should().BeFalse();

        goodServiceCall.Run()
            .Should().Be(Ok("Employee-9"));
    }

    [Fact]
    public void Try_Wrapper()
    {
        Try(() => ExternalService.GetName(9))
            .Run()
            .Should().Be(Ok("Employee-9"));

        Try(() => ExternalService.GetName(11)).Run().IsSuccess
            .Should().BeFalse();

        Try(() => ExternalService.GetName(11))
            .Run().Match(
                success: s => s,
                ex: e => e.Message)
            .Should().Be("Boom");
    }

    private static Try<string> FlakyServiceCall(int id) => () => ExternalService.GetName(id);
}

public static class ExternalService
{
    public static string GetName(int id) =>
        id <= 10 ? $"Employee-{id}" : throw new Exception("Boom");
}
