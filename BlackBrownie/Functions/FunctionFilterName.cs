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

    public async Task Do(string[] args)
    {
        var targetDirRaw = args[0];

        var targetInfo = new DirectoryInfo(targetDirRaw);
        if (!targetInfo.Exists)
        {
            Console.WriteLine($"not dir {targetDirRaw}");
            return;
        }

        var filterWord = args[1];
        if (string.IsNullOrEmpty(filterWord))
        {
            Console.WriteLine($"filter");
            return;
        }

        var fileInfos = targetInfo.GetFiles("*", SearchOption.AllDirectories);
        var towardDir = targetInfo.CreateSubdirectory(filterWord);
        if (!towardDir.Exists)
        {
            towardDir.Create();
        }

        foreach (var f in fileInfos)
        {
            var fileName = f.Name;
            if (!fileName.Contains(filterWord, StringComparison.InvariantCultureIgnoreCase))
            {
                continue;
            }

            var to = Path.Combine(towardDir.FullName, fileName);
            f.CopyTo(to, false);
        }
    }
}
