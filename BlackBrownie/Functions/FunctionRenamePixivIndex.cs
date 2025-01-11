namespace BlackBrownie.Functions;

public sealed class FunctionRenamePixivIndex : IFunction
{
    public string DescriptionFunction()
    {
        return "rename pixiv index p1 -> p01";
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

        var array = Enumerable.Range(0, 9).ToArray();
        var dictionary = array.ToDictionary(i => $"_p{i}_", i => $"_p{i:00}_");
        var fileInfos = targetInfo.GetFiles("*", SearchOption.AllDirectories);

        foreach (var f in fileInfos)
        {
            foreach (var (k, v) in dictionary)
            {
                if (f.Name.Contains(k))
                {
                    var replace = f.FullName.Replace(k, v);
                    f.MoveTo(replace);
                    break;
                }
            }
        }
    }
}
