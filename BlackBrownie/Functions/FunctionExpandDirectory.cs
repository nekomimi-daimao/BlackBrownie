namespace BlackBrownie.Functions;

public sealed class FunctionExpandDirectory : IFunction
{
    public string DescriptionFunction()
    {
        return "open child dir and create _opened";
    }

    public string DescriptionArgs()
    {
        return "targetDir, all";
    }

    public async Task Do(string[] args, CancellationToken token)
    {
        await Task.CompletedTask;

        var targetDirRaw = args[0];
        var allRaw = args.Length > 1;

        var targetInfo = new DirectoryInfo(targetDirRaw);
        if (!targetInfo.Exists)
        {
            Console.WriteLine($"not dir {targetDirRaw}");
            return;
        }

        const string openedDir = "_opened";
        var mergedDir = targetInfo.CreateSubdirectory(openedDir);
        var mergedPath = mergedDir.FullName;
        var searchOption = allRaw ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var directories = targetInfo.GetDirectories("*", searchOption)
            .Where(info => !string.Equals(info.Name, openedDir));

        foreach (var cd in directories)
        {
            Console.WriteLine(cd.Name);
            foreach (var file in cd.EnumerateFiles())
            {
                if (string.Equals(file.Extension, ".ini")
                    || string.Equals(file.Extension, ".zip")
                    || string.Equals(file.Name, ".DS_Store"))
                {
                    continue;
                }

                var path = Path.Combine(mergedPath, $"{cd.Name}_{file.Name.PadLeft(2, '0')}");
                if (Path.Exists(path))
                {
                    continue;
                }

                file.CopyTo(path);
            }
        }
    }
}
