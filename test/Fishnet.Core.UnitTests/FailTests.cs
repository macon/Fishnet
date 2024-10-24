using FluentAssertions;

namespace Fishnet.Core.UnitTests;

public class FailTests
{
    [Fact]
    public void test1()
    {
        Error("Boom!").IsError.Should().BeTrue();
        Exception<string>(new Exception()).IsException.Should().BeTrue();
        Exception<string>(new Exception()).IsException.Should().BeTrue();
    }

    [Fact]
    public void BindTests()
    {
        var x = Exception<int>(new Exception())
            .Bind(i => new Fail<string>($"Boom! {i}"));

        x.IsError.Should().BeFalse();
        x.IsException.Should().BeTrue();
    }
}
