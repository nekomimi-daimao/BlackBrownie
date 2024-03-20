using System.Text;
using System.Text.RegularExpressions;

namespace BlackBrownie.Functions;

public partial class FunctionCompareEditorConfig : IFunction
{
    public string DescriptionFunction()
    {
        return "show .editorconfig diff";
    }

    public string DescriptionArgs()
    {
        return ".editorconfig A, .editorconfig B, result txt dir";
    }

    private const string RegexPattern = @"\[.+\]";
    private readonly Regex _regex = MyRegex();

    public async Task Do(string[] args)
    {
        var configARaw = args[0];
        var configBRaw = args[1];
        var resultRaw = args[2];

        var fileInfoA = new FileInfo(configARaw);
        if (!fileInfoA.Exists)
        {
            Console.WriteLine($"not file {configARaw}");
            return;
        }

        var fileInfoB = new FileInfo(configBRaw);
        if (!fileInfoB.Exists)
        {
            Console.WriteLine($"not file {configBRaw}");
            return;
        }

        var allLinesA = await File.ReadAllLinesAsync(fileInfoA.FullName);
        var a = Parse(allLinesA);
        var allLinesB = await File.ReadAllLinesAsync(fileInfoB.FullName);
        var b = Parse(allLinesB);

        var configsA = a.ToDictionary(config => config.Target);
        var configsB = b.ToDictionary(config => config.Target);

        var keyIntersect = configsA.Keys.Intersect(configsB.Keys);

        var compare = new List<CompareResult>();

        foreach (var k in keyIntersect)
        {
            var vA = configsA[k].Value;
            var vB = configsB[k].Value;

            var both = vA.Keys.Intersect(vB.Keys);
            var diff = new List<Diff>();
            foreach (var keyBoth in both)
            {
                var sA = vA[keyBoth];
                var sB = vB[keyBoth];
                if (string.Equals(sA, sB))
                {
                    continue;
                }

                var d = new Diff
                {
                    Key = keyBoth,
                    ValueA = sA,
                    ValueB = sB
                };
                diff.Add(d);
            }

            var onlyA = vA.Keys.Except(vB.Keys).Select(s =>
                new Diff
                {
                    Key = s,
                    ValueA = vA[s],
                }).ToArray();
            var onlyB = vB.Keys.Except(vA.Keys).Select(s =>
                new Diff
                {
                    Key = s,
                    ValueB = vB[s],
                }).ToArray();

            var compareResult = new CompareResult
            {
                TargetExtension = k,
                Diff = diff.ToArray(),
                OnlyA = onlyA,
                OnlyB = onlyB,
            };
            compare.Add(compareResult);
        }

        const string separator = "-----";
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"A : {configARaw}");
        stringBuilder.AppendLine($"B : {configBRaw}");
        stringBuilder.AppendLine(separator);
        stringBuilder.AppendLine();

        foreach (var c in compare)
        {
            stringBuilder.AppendLine(separator);
            stringBuilder.AppendLine(c.TargetExtension);
            stringBuilder.AppendLine(separator);
            stringBuilder.AppendLine();
            
            if (c.Diff.Length != 0)
            {
                stringBuilder.AppendLine(separator);
                stringBuilder.AppendLine(nameof(c.Diff));
                stringBuilder.AppendLine(separator);
                stringBuilder.AppendLine();
                foreach (var d in c.Diff)
                {
                    stringBuilder.AppendLine(d.Key);
                    stringBuilder.AppendLine(d.ValueA);
                    stringBuilder.AppendLine(d.ValueB);
                    stringBuilder.AppendLine();
                }
            }

            if (c.OnlyA.Length != 0)
            {
                stringBuilder.AppendLine(separator);
                stringBuilder.AppendLine(nameof(c.OnlyA));
                stringBuilder.AppendLine(separator);
                stringBuilder.AppendLine();
                foreach (var oa in c.OnlyA)
                {
                    stringBuilder.AppendLine(oa.Key);
                    stringBuilder.AppendLine(oa.ValueA);
                    stringBuilder.AppendLine();
                }
            }

            if (c.OnlyB.Length != 0)
            {
                stringBuilder.AppendLine(separator);
                stringBuilder.AppendLine(nameof(c.OnlyB));
                stringBuilder.AppendLine(separator);
                stringBuilder.AppendLine();

                foreach (var ob in c.OnlyB)
                {
                    stringBuilder.AppendLine(ob.Key);
                    stringBuilder.AppendLine(ob.ValueB);
                    stringBuilder.AppendLine();
                }
            }

            await File.WriteAllTextAsync(resultRaw, stringBuilder.ToString());
        }
    }

    private List<ParsedEditorConfig> Parse(IEnumerable<string> config)
    {
        var all = new List<ParsedEditorConfig>();
        ParsedEditorConfig? current = null;

        foreach (var s in config)
        {
            if (s.StartsWith($"#"))
            {
                continue;
            }

            var match = _regex.Match(s);
            if (match.Success)
            {
                var value = match.Value;
                var cache = all.FirstOrDefault(c => string.Equals(c.Target, value));
                if (cache == null)
                {
                    cache = new ParsedEditorConfig
                    {
                        Target = value
                    };
                    all.Add(cache);
                }

                current = cache;
            }

            if (current == null)
            {
                continue;
            }

            var kv = s.Split("=");
            if (kv.Length != 2)
            {
                continue;
            }

            current.Value[kv[0].Trim()] = kv[1].Trim();
        }

        return all;
    }

    private sealed record ParsedEditorConfig
    {
        public string Target = string.Empty;
        public readonly Dictionary<string, string> Value = new();
    }

    private sealed record Diff
    {
        public string Key = string.Empty;
        public string ValueA = string.Empty;
        public string ValueB = string.Empty;
    }

    private sealed record CompareResult
    {
        public string TargetExtension = string.Empty;
        public Diff[] Diff = Array.Empty<Diff>();
        public Diff[] OnlyA = Array.Empty<Diff>();
        public Diff[] OnlyB = Array.Empty<Diff>();
    }

    [GeneratedRegex(RegexPattern)]
    private static partial Regex MyRegex();
}
