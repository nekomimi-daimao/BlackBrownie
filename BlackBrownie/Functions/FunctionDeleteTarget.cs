using System.Text;
using System.Text.RegularExpressions;

namespace BlackBrownie.Functions;

public class FunctionDeleteTarget : IFunction
{
    public string DescriptionFunction()
    {
        return "delete target. ^.*[targetKeyword].*";
    }

    public string DescriptionArgs()
    {
        return "dir, targetName";
    }

    public async Task Do(string[] args, CancellationToken token)
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

        var regex = new Regex($"^.*{regexRaw}.*");
        Console.WriteLine(regex.ToString());
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
            Console.WriteLine("canceled");
            return;
        }

        foreach (var fileSystemInfo in array)
        {
            fileSystemInfo.Delete();
        }
    }
}
