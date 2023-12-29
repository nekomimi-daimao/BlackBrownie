namespace BlackBrownie.Functions;

public class FunctionUnseal : IFunction
{
    public string DescriptionFunction()
    {
        return "collect all subdirectory files";
    }

    public string DescriptionArgs()
    {
        return "fromDir, toDir";
    }

    public async Task Do(string[] args)
    {
        await Task.CompletedTask;

        var fromRaw = args[0];
        var toRaw = args[1];
        var fromDir = new DirectoryInfo(fromRaw);
        var toDir = new DirectoryInfo(toRaw);
        if (!fromDir.Exists)
        {
            Console.WriteLine($"no dir {fromRaw}");
            return;
        }

        if (!toDir.Exists)
        {
            toDir.Create();
        }

        var dirFullName = toDir.FullName;
        foreach (var fileInfo in fromDir.EnumerateFiles("*", SearchOption.AllDirectories))
        {
            fileInfo.CopyTo(Path.Combine(dirFullName, $"{fileInfo.Directory!.Name}_{fileInfo.Name}"), true);
        }
    }
}
