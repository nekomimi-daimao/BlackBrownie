using System.Threading.Channels;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace BlackBrownie.Functions;

public class FunctionDownsizeImage : IFunction
{
    private const string DownSizedDirSuffix = "_downsize";

    public string DescriptionFunction()
    {
        return "resize image";
    }

    public string DescriptionArgs()
    {
        return "targetDir, limit(mb)";
    }

    public async Task Do(string[] args, CancellationToken token)
    {
        var targetDirRaw = args[0];
        var ratioRaw = args[1];

        var targetDir = new DirectoryInfo(targetDirRaw);
        if (!targetDir.Exists)
        {
            Console.WriteLine($"invalid dir {targetDirRaw}");
            return;
        }

        if (!int.TryParse(ratioRaw, out var ratio))
        {
            Console.WriteLine($"invalid ratio {ratioRaw}");
            return;
        }

        // サポートする画像ファイルの拡張子
        var supportedExtensions = new List<string> { ".jpg", ".jpeg", ".png", ".bmp" };

        var targetDirPath = targetDir.FullName;
        var outputDir = targetDir.CreateSubdirectory($"{targetDir.Name}{DownSizedDirSuffix}");
        if (!outputDir.Exists)
        {
            outputDir.Create();
        }

        var outputDirPath = outputDir.FullName;
        var fileDownsized = outputDir.EnumerateFiles("*", SearchOption.AllDirectories)
            .Where(info => supportedExtensions.Contains(info.Extension.ToLowerInvariant()))
            .Select(info => info.Name)
            .ToList();

        var fileInfos = targetDir.EnumerateFiles("*", SearchOption.AllDirectories)
            .Where(info => supportedExtensions.Contains(info.Extension.ToLowerInvariant()))
            .Where(info => !fileDownsized.Contains(info.Name))
            .ToArray();

        Console.WriteLine(fileInfos.Length);

        var limit = ratio * 1048576;

        var channel = Channel.CreateUnbounded<FileInfo>(new UnboundedChannelOptions
        {
            SingleReader = false,
            SingleWriter = true,
        });

        var channelWriter = channel.Writer;

        foreach (var fileInfo in fileInfos)
        {
            await channelWriter.WaitToWriteAsync(token);
            await channelWriter.WriteAsync(fileInfo, token);
        }

        channelWriter.TryComplete();

        var consumers = Enumerable.Range(0, 8)
            .Select(async _ =>
            {
                while (await channel.Reader.WaitToReadAsync(token))
                {
                    if (channel.Reader.TryRead(out var item))
                    {
                        await Resize(item, targetDirPath, outputDirPath, limit, token);
                    }
                }
            }).ToArray();

        await Task.WhenAll(consumers);

        Console.WriteLine("done");
    }

    private static async Task Resize(
        FileInfo fileInfo,
        string inputDirectoryPath,
        string outputDirectoryPath,
        long limit,
        CancellationToken token)
    {
        try
        {
            Console.WriteLine(fileInfo.FullName);
            var relativePath = Path.GetRelativePath(inputDirectoryPath, fileInfo.FullName);
            var outputInfo = new FileInfo(Path.Combine(outputDirectoryPath, relativePath));
            if (outputInfo.Directory == null)
            {
                throw new ArgumentException(outputDirectoryPath);
            }

            if (!outputInfo.Directory.Exists)
            {
                outputInfo.Directory.Create();
            }

            if (fileInfo.Length < limit)
            {
                fileInfo.CopyTo(outputInfo.FullName, true);
                return;
            }

            using var image = await Image.LoadAsync(fileInfo.OpenRead(), token);
            var resizeRatio = fileInfo.Length / limit;
            var resizeOptions = new ResizeOptions
            {
                Size = new Size((int)(image.Width / resizeRatio), (int)(image.Height / resizeRatio)),
                Mode = ResizeMode.Max,
            };
            image.Mutate(x => x.Resize(resizeOptions));
            await image.SaveAsync(outputInfo.FullName, cancellationToken: token);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}
