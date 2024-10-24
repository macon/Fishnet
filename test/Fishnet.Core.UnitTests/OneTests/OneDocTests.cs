using FluentAssertions;
using Fishnet.Core;

namespace Fishnet.Core.UnitTests.OneTests;

public class OneDocTests
{
    [Fact]
    public void Basics()
    {
        // Declaration and assignment
        One<int, string> ageOrName = 35;
        var zipOrEmail = new One<int, string>("matt@company.com");

        ageOrName
            .Should().Be(new One<int, string>(35));

        ageOrName.IsT1
            .Should().Be(true);
    }

    [Fact]
    public void MatchUsage()
    {
        One<int, string> ageOrName = 35;

        ageOrName.Match(
                age => $"Age: {age}",
                name => $"Name: {name}")
            .Should().Be("Age: 35");
    }

    [Fact]
    public void SwitchUsage()
    {
        One<int, string> ageOrName = 35;

        // As
        (ageOrName.Some switch
        {
            Some<int> age => $"Age: {age.Value}",
            Some<string> name => $"Email: {name.Value}",
            _ => throw new ArgumentOutOfRangeException()
        })
            .Should().Be("Age: 35");
    }


}
