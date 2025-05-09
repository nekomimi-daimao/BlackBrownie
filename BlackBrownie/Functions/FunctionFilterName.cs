namespace BlackBrownie.Functions;

public class FunctionFilterName : IFunction
{
    public string DescriptionFunction()
    {
        return "filter filename";
    }

    public string DescriptionArgs()
    {
        return "targetDir, filterWord";
    }

    public Task Do(string[] args, CancellationToken token)
    {
        var targetDirRaw = args[0];

        var targetInfo = new DirectoryInfo(targetDirRaw);
        if (!targetInfo.Exists)
        {
            Console.WriteLine($"not dir {targetDirRaw}");
            return Task.CompletedTask;
        }

        var filterWord = args[1];
        if (string.IsNullOrEmpty(filterWord))
        {
            Console.WriteLine($"filter");
            return Task.CompletedTask;
        }

        var fileInfos = targetInfo.GetFiles("*", SearchOption.AllDirectories);
        var towardDir = targetInfo.CreateSubdirectory(filterWord);
        if (!towardDir.Exists)
        {
            towardDir.Create();
        }

        foreach (var f in fileInfos)
        {
            if (token.IsCancellationRequested)
            {
                return Task.CompletedTask;
            }

            var fileName = f.Name;
            if (!fileName.Contains(filterWord, StringComparison.InvariantCultureIgnoreCase))
            {
                continue;
            }

            var to = Path.Combine(towardDir.FullName, fileName);
            f.CopyTo(to, true);
        }

        return Task.CompletedTask;
    }
}
