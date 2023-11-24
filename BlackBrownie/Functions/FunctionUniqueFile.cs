using System.Security.Cryptography;
using System.Text;

namespace BlackBrownie.Functions;

public sealed class FunctionUniqueFile : IFunction
{
    public string DescriptionFunction()
    {
        return "check file MD5hash, delete duplicates";
    }

    public string DescriptionArgs()
    {
        return "targetDir";
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

        var cache = new HashSet<string>();
        using var csp = MD5.Create();
        var hashStr = new StringBuilder();

        var fileInfos = targetInfo.GetFiles();
        foreach (var file in fileInfos)
        {
            await using var fileStream = file.Open(FileMode.Open, FileAccess.Read);
            var computeHash = await csp.ComputeHashAsync(fileStream);
            fileStream.Close();
            hashStr.Clear();
            foreach (var hashByte in computeHash)
            {
                hashStr.Append(hashByte.ToString("x2"));
            }

            var add = cache.Add(hashStr.ToString());

            if (!add)
            {
                file.Delete();
            }
        }
    }
}
