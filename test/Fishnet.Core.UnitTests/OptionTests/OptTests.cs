using FluentAssertions;
using Fishnet.Core;
using Xunit.Abstractions;

namespace Fishnet.Core.UnitTests.OptionTests;
#nullable enable
#nullable restore
public class OptTests(ITestOutputHelper logger)
{
    [Fact]
    public void SomeTests()
    {
        var stringOpt = Some("John");
        stringOpt.Should().Be(Some("John"));

        var intOpt = Some(123);
        intOpt.Should().Be(Some(123));

        var anotherIntOpt = Some(123);
        anotherIntOpt.Should().BeEquivalentTo(intOpt);

        Assert.Throws<ArgumentNullException>(() => Some<string>(null!));
    }

    [Fact]
    public void EqualityTests()
    {
        Some(12)
            .Should().Be(Some(12));

        Some(14)
            .Should().NotBe(Some(12));

        Some("Foo")
            .Should().NotBe(Some(12));

        None.Of<int>()
            .Should().Be(None.Of<int>());

        None.Of<int>()
            .Should().NotBe(None.Of<string>());
    }

    [Fact]
    public void ImplicitConversionTests()
    {
        Opt<string> stringOpt = "John";
        stringOpt.Should().Be(Some("John"));

        Opt<int> intOpt = 123;
        intOpt.Should().Be(Some(123));
    }

    [Theory]
    [InlineData("John", "John")]
    [InlineData(null, "")]
    public void MatchTests(string? boundValue, string expectedSignal)
    {
        Opt<string> stringOpt = boundValue;

        // Match(Func<>) overload
        var signal = stringOpt.Match(
            some: s => s,
            none: () => "");

        signal.Should().Be(expectedSignal);

        var action = "null";
        // Match(Action<>) overload
        stringOpt.Match(
            none: () => action = "",
            some: s => action = s);

        action.Should().Be(expectedSignal);
    }

    [Fact]
    public void MapTests()
    {
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

    [Fact]
    public void LinqExpressions()
    {
        (from x in Some(4)
         select x * 2)
            .Should().Be(Some(8));
    }

    [Fact]
    public void SelectMany()
    {
        var expected = new List<int> { 12, 10, 12 };
        var fruits = Some(new List<string> { "banana", "apple", "orange" });

        var result = fruits.SelectMany(
            f => Some(f.Select(s => s.Length).ToList()),
            (f, l) => l.Select(n => n * 2).ToList());

        result.Match(
                none: Enumerable.Empty<int>,
                some: l => l)
            .Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void BindTests()
    {
        Some("John")
            .Bind(LenIfJohn)
            .Should().Be(Some(4));

        Some("Ben")
            .Bind(LenIfJohn)
            .Should().Be(None);

        Some(2)
            .Bind(DoubleIfTwo)
            .Should().Be(Some(4));

        Some(3)
            .Bind(DoubleIfTwo)
            .Should().Be(None);

        None.Of<int>()
            .Bind(DoubleIfTwo)
            .Should().Be(None);
        return;

        Opt<int> LenIfJohn(string s) => s == "John" ? Some(s.Length) : None;
        Opt<int> DoubleIfTwo(int i) => i == 2 ? Some(i * 2) : None;
    }

    [Fact]
    public void IsSomeTests()
    {
        var stringSome = Some("John");
        var stringNone = None.Of<string>();
        var intSome = Some(123);
        Opt<int> intNone = None;

        stringSome.IsSome.Should().BeTrue();
        stringNone.IsSome.Should().BeFalse();
        intSome.IsSome.Should().BeTrue();
        intNone.IsSome.Should().BeFalse();
    }

    [Fact]
    public void IsNoneTests()
    {
        var stringSome = Some("John");
        var stringNone = None.Of<string>();
        var intSome = Some(123);
        var intNone = None.Of<int>();

        stringSome.IsNone.Should().BeFalse();
        stringNone.IsNone.Should().BeTrue();
        intSome.IsNone.Should().BeFalse();
        intNone.IsNone.Should().BeTrue();
    }

    [Fact]
    public void NoneTests()
    {
        Opt<string>.None.Should().BeEquivalentTo(None);
        None.Of<string>().Should().BeEquivalentTo(None);

        var noneOpt = Opt<string>.None;
        noneOpt.Should().Be(None);

        var intNone = None.Of<int>();
        intNone.Should().NotBeEquivalentTo(noneOpt);
    }

    [Fact]
    public void Explore1()
    {
        var z = Some(3);

        if (z is { IsSome: true } some)
        {
            logger.WriteLine($"x is {some}");
        }
    }

    public class ApplyTests
    {
        private readonly Func<int, int, int> mult = (x, y) => x * y;
        private readonly Func<int, int, int, int> multThenDiv = (x, y, z) => x * y / z;

        [Fact]
        public void ApplyUsingMap()
        {
            // What's going on here?
            Some(3)                         // -> Opt<int>
                .Map(mult)                  // -> Opt<Func<int, int>>
                .Apply(Some(2))             // -> Opt<int>
                .Should().Be(Some(6));

            // Let's break it down...

            /* A standard Opt<int> containing 3.
             */
            var x1 = Some(3);               // -> Opt<int>

            /* Converts 'mult', a Func<int, int, int>, into a Func<int, Func<int, int>>.
             * (See CurryTests for more info.)
             */
            var x2 = mult.Curry();          // -> Func<int, Func<int, int>>

            /*
             * Remember: Map is like Select.
             * But this overload recognises that the function requires 2 arguments,
             * so it applies Curry to it first wrapping it in the Func<int, Func<int, int>>.
             *
             * After passing the Opt's inner value (3) to the curried function the result is:
             *  Opt<Func<int, int>> i.e. Some(y => 3 * y)
             */
            var x3 = x1.Map(x2);            // -> Opt<Func<int, int>>

            /* Apply takes a M<T> and a M<Func<T, R>> and returns M<R>.
             * The output of our last step was Opt<Func<int, int>> i.e. Some(y => 3 * y) so this is our M<Func<T, R>>.
             * We're passing in Some(2) which is our M<T> i.e. Opt<int>.
             *
             * So evaluating this we actually execute the inner (curried) function (x * y) resulting in Some(6).
             */
            var x4 = x3.Apply(Some(2));     // -> Opt<int>

            x4.Should().Be(Some(6));
        }

        [Fact]
        public void ApplyWithArity3()
        {
            Some(5)                         // -> Opt<int>
                .Map(multThenDiv)           // -> Opt<Func<int, int, int>>
                .Apply(Some(2))             // -> Opt<Func<int, int>>
                .Apply(Some(5))             // -> Opt<int>
                .Should().Be(Some(2));
        }

        [Fact]
        public void ApplyArgsToLiftedFunc()
        {
            // You don't need to start with one of the arguments.
            // You can start with the lifted function and then apply the arguments.
            Some(mult)                      // -> Opt<Func<int, int, int>>
                .Apply(Some(3))             // -> Opt<Func<int, int>>
                .Apply(Some(4))             // -> Opt<int>
                .Should().Be(Some(12));
        }
    }

    [Theory(Skip = "this may be breaking xunit")]
    [InlineData("1, 2,  7", 10)]
#pragma warning disable xUnit1012
    [InlineData("1, 2,  XX", null)]
#pragma warning restore xUnit1012
    public void TraverseTests(string nums, Opt<int> expectedResult)
    {
        Opt<int> IntParse(string s) => int.TryParse(s, out var i) ? Some(i) : None;

        // Act
        var theProblem =
            nums.Split(',')                 // => string[]
                .Select(s => s.Trim())      // => IEnumerable<string>
                .Select(IntParse);          // => IEnumerable<Opt<int>>
        // We now have IEnumerable<Opt<int>> but we want an Opt<IEnumerable<int>>

        var theSolution =
            nums.Split(',')                 // string[]
                .Select(s => s.Trim())      // IEnumerable<string>
                .Traverse(IntParse)         // Opt<IEnumerable<int>>
                .Map(Enumerable.Sum);       // Opt<int>

        theSolution.Should().Be(expectedResult);

        /* Basically, Traverse() converts a IEnumerable<Opt<T>> into an Opt<IEnumerable<T>>.
         *
         * Traverse works on collections. Its definition is:
         *
         *  C<T> -> Func<T, M<R>> -> M<C<R>>
         *  or more specifically:
         *  IEnumerable<string> -> Func<string, Opt<int>> -> Opt<IEnumerable<int>>
         *
         * It takes a collection e.g. IEnumerable<string> and a Func<int, Opt<int>>.
         * Ts =   ["1", "2", "3"]
         * func = s => int.TryParse(s, out var i) ? Some(i) : None
         *
         * Internally it uses Aggregate() to fold the collection into a single Opt.
         *
         * Ts.Aggregate(
         *      seed: Some(Enumerable.Empty<int>()),
         *      func: (optRs, t) =>
         *          from rs in optRs
         *          from r in func(t)
         *          select rs.Append(r));
         */
    }
}
