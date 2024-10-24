namespace Fishnet.SourceGen;

public interface IParser<out T>
{
    /// <summary>
    /// Runs the parser on the given input.
    /// </summary>
    T Parse(int arity, string input);
}

public class InlineExpandoParser : IParser<IEnumerable<string>>
{
    public IEnumerable<string> Parse(int arity, string input)
    {
        for (var i = 1; i <= arity; i++)
        {
            var x = input.Replace("@NUM@", i.ToString());
            yield return x;
        }
    }
}
