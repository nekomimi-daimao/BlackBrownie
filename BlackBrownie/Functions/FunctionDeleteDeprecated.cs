namespace BlackBrownie.Functions;

public sealed class FunctionDeleteDeprecated : IFunction
{
    public string DescriptionFunction()
    {
        return "Delete (1) files";
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

        foreach (var fileInfo in targetInfo.GetFiles())
        {
            if (!fileInfo.Name.Contains("(1)"))
            {
                continue;
            }

            Console.WriteLine(fileInfo.FullName);

            fileInfo.Delete();
        }
    }
}