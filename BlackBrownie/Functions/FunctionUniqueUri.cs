namespace BlackBrownie.Functions;

public class FunctionUniqueUri : IFunction
{
    public string DescriptionFunction()
    {
        return "delete duplicate uri, create new";
    }

    public string DescriptionArgs()
    {
        return "target.txt";
    }

    public async Task Do(string[] args)
    {
        var fileInfo = new FileInfo(args[0]);
        var fileDir = fileInfo.Directory;
        if (!fileInfo.Exists || fileDir == null)
        {
            Console.WriteLine($"no file {args[0]}");
            return;
        }

        var dic = new Dictionary<string, int>();
        var streamReader = fileInfo.OpenText();
        while (streamReader.Peek() > -1)
        {
            var readLine = await streamReader.ReadLineAsync();
            if (string.IsNullOrEmpty(readLine))
            {
                continue;
            }

            Uri uri;
            try
            {
                uri = new Uri(readLine);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                continue;
            }

            var u = uri.GetLeftPart(UriPartial.Path);
            if (dic.TryGetValue(u, out var count))
            {
                dic[u] = count + 1;
            }
            else
            {
                dic[u] = 1;
            }
        }

        foreach (var (key, value) in dic
                     .Where(pair => pair.Value > 1)
                     .OrderByDescending(pair => pair.Value))
        {
            Console.WriteLine($"[{value}] : {key}");
        }

        await File.WriteAllLinesAsync(Path.Combine(fileDir.FullName, $"uni_{fileInfo.Name}"), dic.Keys.Order());
    }
}
