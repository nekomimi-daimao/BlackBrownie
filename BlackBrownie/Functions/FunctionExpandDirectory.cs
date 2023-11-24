namespace BlackBrownie.Functions;

public sealed class FunctionExpandDirectory : IFunction
{
    public string DescriptionFunction()
    {
        return "open child dir and create _opened";
    }

    public string DescriptionArgs()
    {
        return "targetDir";
    }

    public async Task Do(string[] args)
    {
        await Task.CompletedTask;

        var targetDirRaw = args[0];

        var targetInfo = new DirectoryInfo(targetDirRaw);
        if (!targetInfo.Exists)
        {
            Console.WriteLine($"not dir {targetDirRaw}");
            return;
        }

        const string openedDir = "_opened";
        var mergedDir = targetInfo.CreateSubdirectory(openedDir);
        var mergedPath = mergedDir.FullName;
        var directories = targetInfo.GetDirectories().Where(info => !string.Equals(info.Name, openedDir));

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
