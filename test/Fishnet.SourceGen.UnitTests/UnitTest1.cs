using Xunit.Abstractions;

namespace Fishnet.SourceGen.UnitTests;

public class UnitTest1
{
    private readonly ITestOutputHelper _logger;

    public UnitTest1(ITestOutputHelper logger)
    {
        _logger = logger;
    }

    [Fact(Skip = "This test is for demonstration purposes only.")]
    public void Test1()
    {
        _logger.WriteLine($"{OneTypeBuilder.BuildType(4)}");
    }

    [Fact(Skip = "This test is for demonstration purposes only.")]
    public void Test2()
    {
        var result = BuildFromResource.BuildIt(4);
        _logger.WriteLine($"{result}");
    }
}
