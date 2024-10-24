using FluentAssertions;
using Fishnet.Core;

namespace Fishnet.Core.UnitTests.OptionTests;

public class OptDocTests
{
    [Fact]
    public void Basics()
    {
        // Declaration and assignment
        var user2 = Some("Matt");               // static constructor method: () => Opt<string>
        Opt<string> user1 = "Matt";             // implicit cast: string => Opt<string>
        Opt<string> dodgyUser = None;           // special NoneType casts to all Opt<T>

        // Value equality
        user1.Should().Be(user2);
        user1.Should().NotBe(dodgyUser);

        // Nulls are not allowed
        Assert.Throws<ArgumentNullException>(() => Some<string>(null!));
    }

    [Fact]
    public void MatchUsage()
    {
        // Match allows you to map the Opt<T> to a new type.
        // In this case Opt<int> => bool.
        Some(9)
            .Match(
                some: n => n > 10,
                none: () => false)
            .Should()
            .Be(false);

        Some(12)
            .Match(
                some: n => n > 10,
                none: () => false)
            .Should()
            .Be(true);
    }

    [Fact]
    public void MapUsage()
    {
        // Map applies a function if the Opt is Some.
        // The mapping function must return another Opt<T> (but T can be different).
        // If the Opt<T> is None, the mapping function is not called and None is returned.
        // In this case the mapping is Opt<string> => Opt<int>.
        Some("John")
            .Map(s => s.Length)
            .Should().Be(Some(4));

        Some(2)
            .Map(i => i * 2)
            .Should().Be(Some(4));

        None.Of<int>()
            .Map(i => i * 2)
            .Should().Be(None);
    }
}
