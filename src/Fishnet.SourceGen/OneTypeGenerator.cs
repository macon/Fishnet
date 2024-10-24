using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Fishnet.SourceGen;

[Generator]
public class OneTypeGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // throw new ApplicationException();
        context.RegisterPostInitializationOutput(static ctx =>
        {
            ctx.AddSource("OneTypeOf2.g.cs",
                SourceText.From(BuildFromResource.BuildIt(2), Encoding.UTF8));

            ctx.AddSource("OneTypeOf3.g.cs",
                SourceText.From(BuildFromResource.BuildIt(3), Encoding.UTF8));
            // ctx.AddSource("OneTypeOf4.g.cs",
            //     SourceText.From(OneTypeBuilder.BuildType(4), Encoding.UTF8));
        });
    }
}

public static class OneTypeBuilder
{
    public static string BuildType(uint arity)
    {
        var sb = new StringBuilder();
        sb.AppendLine("namespace Fishnet.Core;");
        sb.AppendLine(BuildDeclaration(arity));
        sb.AppendLine("{");
        foreach (var opt in BuildOpts(arity))
        {
            sb.AppendLine(opt);
        }
        sb.AppendLine("}");
        Console.WriteLine(sb.ToString());
        return sb.ToString();
    }

    public static string GetTs(uint arity) => $"{string.Join(", ", Enumerable.Range(1, (int)arity).Select(i => $"T{i}"))}";

    public static string BuildDeclaration(uint arity) =>
        $"public readonly record struct One<{GetTs(arity)}>";

    public static IEnumerable<string> BuildOpts(uint arity)
    {
        for (var i = 1; i <= arity; i++)
        {
            yield return $"public Opt<T{i}> Opt{i} {{get;}}";
        }
    }
}

public static class BuildFromResource
{
    public static string BuildIt(uint arity)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceText = assembly.ReadResourceAsync("Source.One.txt", arity);
        return resourceText;
    }

    private static string ReadResourceAsync(this Assembly assembly, string name, uint arity)
    {
        var resourceName = assembly.GetName().Name + "." + name;
        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using StreamReader reader = new(stream);

        var sb = new StringBuilder();
        var lineBuffer = new List<string>();
        var seeking = false;

        while (reader.ReadLine() is { } line)
        {
            if (ContainsMultilineSymbol(line))
            {
                lineBuffer.Add(line);
                seeking = true;
                continue;
            }

            if (seeking)
            {
                lineBuffer.Add(line);

                if (!ContainsEndSymbol(line)) { continue; }
                seeking = false;
            }
            else
            {
                lineBuffer.Add(line);
            }

            var output = new List<string>();
            if (lineBuffer.Count > 1)
            {
                Console.WriteLine("Before ParseMultiline");
                output.AddRange(ParseMultiline(lineBuffer, arity));
                Console.WriteLine("After ParseMultiline");
                lineBuffer.Clear();
            }
            else
            {
                Console.WriteLine("Before ParseLine");
                output.AddRange(ParseLine(lineBuffer, arity));
                Console.WriteLine("After ParseLine");
            }

            foreach (var item in output)
            {
                sb.AppendLine(item);
            }
            output.Clear();
            lineBuffer.Clear();
            Console.WriteLine("Main loop");
        }
        return sb.ToString();
    }

    private static IEnumerable<string> ParseLine(IEnumerable<string> lines, uint arity)
    {
        var enumerable = lines.ToList();
        if (!ContainsPlaceholders(enumerable))
        {
            return enumerable;
        }

        var newLines = new List<string>();

        if (enumerable.Count > 1)
        {
            newLines.AddRange(enumerable.Select(l => ApplyExpanders(l, arity)));
            enumerable = newLines;
            newLines.Clear();
        }

        foreach (var line in enumerable)
        {
            newLines.AddRange(ParserApply(line, arity));
        }

        return ParseLine(newLines, arity);
    }

    private static IEnumerable<string> ParseMultiline(IEnumerable<string> lines, uint arity)
    {
        return RepeatMultiLine(lines, arity);
    }

    private static readonly IList<string> Placeholders = new List<string>
    {
        "|repeat_csv|",
        "|repeat_line|",
        "|repeat_line_csv|",
        "|repeat_multi_line_csv|",
        "|repeat_multi_line_semicolon|"
    };

    private static bool ContainsPlaceholders(IEnumerable<string> lines) => lines.Any(l => Placeholders.Any(l.Contains));
    private static bool ContainsPlaceholders(string line) => Placeholders.Any(line.Contains);
    private static bool ContainsMultilineSymbol(string line)
        => line.Contains("|repeat_multi_line_csv|") || line.Contains("|repeat_multi_line_semicolon|");

    private static bool ContainsEndSymbol(string line) => line.TrimEnd().EndsWith("|");

    private static string ApplyExpanders(string line, uint arity)
    {
        return ApplyRepeatCsv(line, arity);
    }

    private static IEnumerable<string> ParserApply(string line, uint arity)
    {
        var res = ApplyRepeatCsv(line, arity);
        var res2 = RepeatLine(res, arity);
        foreach (var innerLine in res2)
        {
            foreach (var resultLine in RepeatLineCsv(innerLine, arity))
            {
                yield return resultLine;
            }
        }
    }

    private static IEnumerable<string> RepeatLine(string line, uint arity)
    {
        const string symbolPrefix = "|repeat_line|";

        if (!line.Contains(symbolPrefix))
        {
            yield return line;
            yield break;
        }

        var infix = line.Replace(symbolPrefix, "");

        var res = Repeat(infix, arity);

        foreach (var item in res)
        {
            yield return item;
        }
    }

    private static IEnumerable<string> RepeatMultiLine(IEnumerable<string> lines, uint arity)
    {
        const string symbolPrefix1 = "|repeat_multi_line_csv|";
        const string symbolPrefix2 = "|repeat_multi_line_semicolon|";
        var separator = ",";

        var lineList = lines.ToList();

        if (!lineList.First().Contains(symbolPrefix1) && !lineList.First().Contains(symbolPrefix2))
        {
            return lineList;
        }

        if (lineList.First().Contains(symbolPrefix2))
        {
            separator = ";";
        }

        var infixed = new List<string> { ApplyExpanders(lineList.First().Replace(symbolPrefix1, "").Replace(symbolPrefix2, ""), arity) };
        lineList
            .Where((_, i) => i > 0 && i < lineList.Count - 1)
            .ToList()
            .ForEach(l => infixed.Add(ApplyExpanders(l, arity)));
        infixed.Add(ApplyExpanders(lineList.Last().TrimEnd('|'), arity));

        var masterSet = new List<List<string>>();
        var expandedSet = new List<string>();

        for (uint i = 1; i <= arity; i++)
        {
            foreach (var item in infixed)
            {
                var x = RepeatCsvIntersect(item, arity, i);
                expandedSet.Add(ExpandForArity(x, i));
            }

            if (i < arity)
            {
                var x = expandedSet.Last() + separator;
                expandedSet.RemoveAt(expandedSet.Count - 1);
                expandedSet.Add(x);
            }
            else
            {
                if (separator == ";")
                {
                    var x = expandedSet.Last() + separator;
                    expandedSet.RemoveAt(expandedSet.Count - 1);
                    expandedSet.Add(x);
                }
            }

            var y = new List<string>();
            expandedSet.ForEach(s => y.Add((string)s.Clone()));
            masterSet.Add(y);
            expandedSet.Clear();
        }

        return masterSet.SelectMany(m => m);
    }

    private static IEnumerable<string> RepeatLineCsv(string line, uint arity)
    {
        const string symbolPrefix = "|repeat_line_csv|";

        if (!line.Contains(symbolPrefix))
        {
            yield return line;
            yield break;
        }

        var infix = line.Replace(symbolPrefix, "");

        var res = Repeat(infix, arity).ToList();
        var res2 = new List<string>();
        foreach (var (re, idx) in res.Select((s, i) => (s, i)))
        {
            res2.Add($"{re}{(idx < res.Count - 1 ? "," : "")}");
        }

        foreach (var item in res2)
        {
            yield return item;
        }
    }

    private static string ApplyRepeatCsv(string line, uint arity)
    {
        var response = line;
        while (response.Contains("|repeat_csv|"))
        {
            response = RepeatCsv(response, arity);
        }

        return response;
    }

    private static string RepeatCsv(string line, uint arity)
    {
        const string symbolPrefix = "|repeat_csv|";
        const string symbolSuffix = "|";

        if (!line.Contains(symbolPrefix)) { return line; }

        var start = line.IndexOf(symbolPrefix, StringComparison.InvariantCultureIgnoreCase);
        var end = line.IndexOf(symbolSuffix, start + symbolPrefix.Length, StringComparison.InvariantCultureIgnoreCase);
        var linePrefix = line.Substring(0, start);
        var lineSuffix = line.Substring(end + symbolSuffix.Length);
        var infix = line.Substring(start + symbolPrefix.Length, end - start - symbolPrefix.Length);

        var res = Repeat(infix, arity);

        var types = string.Join(", ", res);
        return $"{linePrefix}{types}{lineSuffix}";
    }

    private static string RepeatCsvIntersect(string line, uint arity, uint outerArity)
    {
        const string symbolPrefix = "|repeat_with_intersect_csv|";
        const string symbolSuffix = "|";

        if (!line.Contains(symbolPrefix)) { return line; }

        var start = line.IndexOf(symbolPrefix, StringComparison.InvariantCultureIgnoreCase);
        var end = GetNthIndex(line, '|', 2, start + symbolPrefix.Length);
        // var end = line.IndexOf(symbolSuffix, start + symbolPrefix.Length, StringComparison.InvariantCultureIgnoreCase);
        var linePrefix = line.Substring(0, start);
        var lineSuffix = line.Substring(end + symbolSuffix.Length);
        var infix = line.Substring(start + symbolPrefix.Length, end - start - symbolPrefix.Length);

        // xxx|repeat_with_intersect_csv|T@n|R@n|yyy
        var parts = infix.Split('|');

        var res = RepeatWithIntersect(parts[0], parts[1], arity, outerArity);

        var types = string.Join(", ", res);
        return $"{linePrefix}{types}{lineSuffix}";
    }

    public static int GetNthIndex(string s, char t, uint n = 1, int start = 0)
    {
        uint count = 0;
        for (var i = start; i < s.Length; i++)
        {
            if (s[i] == t)
            {
                count++;
                if (count == n)
                {
                    return i;
                }
            }
        }
        return -1;
    }

    private static IEnumerable<string> RepeatWithIntersect(string infix, string infixIntersect, uint arity, uint outerArity)
    {
        for (uint i = 1; i <= arity; i++)
        {
            if (i == outerArity)
            {
                yield return ExpandForArity(infixIntersect, i);
            }
            else
            {
                yield return ExpandForArity(infix, i);
            }
        }
    }

    private static IEnumerable<string> Repeat(string infix, uint arity)
    {
        for (uint i = 1; i <= arity; i++)
        {
            yield return ExpandForArity(infix, i);
        }
    }

    private static string ExpandForArity(string infix, uint arity)
    {
        return infix
            .Replace("@n", arity.ToString())
            .Replace("@w", ToWord(arity));
    }

    private static string ToWord(uint number) =>
        number switch
        {
            1U => "One",
            2U => "Two",
            3U => "Three",
            4U => "Four",
            5U => "Five",
            6U => "Six",
            7U => "Seven",
            8U => "Eight",
            9U => "Nine",
            10U => "Ten",
            _ => throw new ArgumentOutOfRangeException(nameof(number))
        };
}

