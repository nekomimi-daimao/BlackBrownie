using System.Text;
using System.Text.RegularExpressions;

namespace BlackBrownie.Functions;

public class FunctionDeleteTarget : IFunction
{
    public string DescriptionFunction()
    {
        return "delete target. ^.*検索文字列.*";
    }

    public string DescriptionArgs()
    {
        return "dir, targetName";
    }

    public async Task Do(string[] args)
    {
        await Task.CompletedTask;

        var targetDirRaw = args[0];
        var regexRaw = args[1];

        var directoryInfo = new DirectoryInfo(targetDirRaw);
        if (!directoryInfo.Exists)
        {
            Console.WriteLine($"no dir {targetDirRaw}");
            return;
        }

        var regex = new Regex($"^.*{regexRaw}.*\"");
        var array = directoryInfo.EnumerateFileSystemInfos("*", SearchOption.AllDirectories)
            .Where(info => regex.IsMatch(info.Name))
            .ToArray();

        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine();
        foreach (var s in array)
        {
            stringBuilder.AppendLine(s.FullName);
        }

        stringBuilder.AppendLine();
        Console.WriteLine(stringBuilder.ToString());

        Console.WriteLine("y or else");
        var readLine = Console.ReadLine();
        if (!string.Equals("y", readLine?.ToLower()))
        {
            Console.WriteLine("stop");
            return;
        }

        foreach (var fileSystemInfo in array)
        {
            fileSystemInfo.Delete();
        }
    }
}
