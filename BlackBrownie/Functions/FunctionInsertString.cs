namespace BlackBrownie.Functions;

public class FunctionInsertString : IFunction
{
    public string DescriptionFunction()
    {
        return "insert string in file top";
    }

    public string DescriptionArgs()
    {
        return "targetDir, insertTxtPath, targetExtension";
    }

    public async Task Do(string[] args)
    {
        var targetDirRaw = args[0];
        var insertTextRaw = args[1];
        var extensionRaw = args[2];
        var targetInfo = new DirectoryInfo(targetDirRaw);
        if (!targetInfo.Exists)
        {
            Console.WriteLine($"not dir {targetDirRaw}");
            return;
        }

        var insertText = await File.ReadAllTextAsync(insertTextRaw);

        foreach (var file in targetInfo.EnumerateFiles($"*.{extensionRaw}", SearchOption.AllDirectories))
        {
            if (!file.Exists)
            {
                return;
            }

            var fileFullPath = file.FullName;
            var allText = await File.ReadAllTextAsync(fileFullPath);
            var all = insertText + allText;
            await File.WriteAllTextAsync(fileFullPath, all);
        }
    }
}
