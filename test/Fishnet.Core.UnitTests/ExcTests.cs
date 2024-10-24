using FluentAssertions;

namespace Fishnet.Core.UnitTests;

public class ExcTests
{
    [Fact]
    public void OkAssignmentTests()
    {
        var okBob = Ok("Bob");
        var badBob = Exc<string>(new Exception());

        new Exc<string>("Bob")
            .Should().Be(okBob)
            .And.NotBe(badBob);

        Exc<string> exc = "Bob";
        exc.Should().Be(okBob)
            .And.NotBe(badBob);

        Ok("Bob")
            .Should().Be(okBob)
            .And.NotBe(badBob);
    }

    [Fact]
    public void BindTests()
    {
        var okBob = Ok("Bob");

        okBob.Bind(s => Ok(s.Length))
            .Should().Be(Ok(3));

        var badBob = Exc<string>(new Exception());
        var badBobAsInt = Exc<int>(new Exception());

        badBob.Bind(s => Ok(s.Length))
            .Should().Be(badBobAsInt);
    }

    [Fact]
    public void ExcAssignmentTests()
    {
        var okBob = Ok("Bob");
        var badBob = Exc<string>(new Exception());

        new Exc<string>(new Exception())
            .Should().Be(badBob)
            .And.NotBe(okBob);

        Exc<string> exc = new Exception();
        exc.Should().Be(badBob)
            .And.NotBe(okBob);

        Exc<string>(new Exception())
            .Should().Be(badBob)
            .And.NotBe(okBob);
    }
}
