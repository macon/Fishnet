using FluentAssertions;

namespace Fishnet.Core.UnitTests;

public class LabTests
{
    [Fact]
    public void test1()
    {
        Func<int, int, int> multNormal = (a, b) => a * b;
        Func<int, Func<int, int>> multCurried = a => b => a * b;
        Func<int, Func<int, int>> multCurriedAlt = a => b => multNormal(a, b);
        var curriedMultNormal = Curry(multNormal);

        var multBy3 = multCurried(3);
        var multBy5 = curriedMultNormal(5);

        multNormal(3, 4).Should().Be(12);
        multBy3(4).Should().Be(12);
        multCurriedAlt(4)(3).Should().Be(12);
        multBy5(3).Should().Be(15);
    }

    [Fact]
    public void test2()
    {
        Func<int, int, int> multNormal = (a, b) => a * b;
        var curriedMultNormal = Curry(multNormal);

        var multBy5 = curriedMultNormal(5);

        multNormal(3, 4).Should().Be(12);
        multBy5(3).Should().Be(15);
    }

    public static Func<T1, Func<T2, TR>> Curry<T1, T2, TR>(Func<T1, T2, TR> f)
        => t1 => t2 => f(t1, t2);

}
