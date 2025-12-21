using System.Text;
using Zx;

namespace BlackBrownie.Functions;

public class FunctionGenerateThumbnail : IFunction
{
    public string DescriptionFunction()
    {
        return "generate thumbnail, generate html";
    }

    public string DescriptionArgs()
    {
        return "movieDir htmlDir";
    }

    public async Task Do(string[] args, CancellationToken token)
    {
        var directoryMovie = new DirectoryInfo(args[0]);
        if (!directoryMovie.Exists)
        {
            Console.WriteLine($"Directory doesn't exist {args[0]}");
            return;
        }

        var directoryHtml = new DirectoryInfo(args[1]);
        if (!directoryHtml.Exists)
        {
            directoryHtml.Create();
        }

        var directoryThumbnail = directoryHtml.CreateSubdirectory("thumbnail");
        var directoryThumbnailFullName = directoryThumbnail.FullName;

        const int width = 640;

        var dicThumbHtml = new Dictionary<string, string>();
        var setError = new SortedSet<string>();

        var directoryTmp = directoryHtml.CreateSubdirectory("tmp");

        foreach (var fi in directoryMovie.EnumerateFiles("*.mp4", SearchOption.AllDirectories))
        {
            var fileInfo = fi;

            try
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                Console.WriteLine($"Process start {fileInfo.FullName}");
                Console.WriteLine();

                var thumbnailName = fileInfo.FullName
                    .Replace(" ", "")
                    .Replace("/", "_")
                    .Replace("'", "");

                var pathOriginal = fileInfo.FullName;

                if (fileInfo.FullName.Contains('\''))
                {
                    var fileTmp = new FileInfo(Path.Combine(directoryTmp.FullName, thumbnailName));
                    fileInfo.CopyTo(fileTmp.FullName, true);
                    fileInfo = fileTmp;
                }

                thumbnailName = Path.ChangeExtension(thumbnailName, ".png");
                var thumbnailPath = Path.Combine(directoryThumbnailFullName, thumbnailName);

                // ffmpeg -i input.mp4 -ss 00:00:01 -vframes 1 -vf "scale=640:-1" thumbnail.png
                var ffmpeg =
                    $"ffmpeg -i '{fileInfo.FullName}' -ss 00:00:01 -vframes 1 -vf scale={width}:-1 -y '{thumbnailPath}' ";
                Console.WriteLine(ffmpeg);
                Console.WriteLine();
                await ffmpeg;

                var b64 = "";
                if (File.Exists(thumbnailPath))
                {
                    var bytes = await File.ReadAllBytesAsync(thumbnailPath, token);
                    b64 = Convert.ToBase64String(bytes);
                }

                if (string.IsNullOrEmpty(b64))
                {
                    setError.Add(fileInfo.FullName);
                }

                dicThumbHtml[pathOriginal] =
                    HtmlTemplateFigure
                        .Replace(PlaceHolderCaption, pathOriginal)
                        .Replace(PlaceHolderImage, $"data:image/png;base64,{b64}");
                Console.WriteLine("Process end");
                Console.WriteLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                setError.Add(fileInfo.FullName);
            }
        }

        var stringBuilder = new StringBuilder();
        var keys = dicThumbHtml.Keys.ToList();
        keys.Sort();
        foreach (var k in keys)
        {
            stringBuilder.AppendLine(dicThumbHtml[k]);
        }

        var stringBuilderError = new StringBuilder();
        var hasError = setError.Count != 0;
        if (hasError)
        {
            stringBuilderError.AppendLine("<ul>");

            foreach (var se in setError)
            {
                stringBuilderError.AppendLine("<li>");
                stringBuilderError.AppendLine(se);
                stringBuilderError.AppendLine("</li>");
            }

            stringBuilderError.AppendLine("</ul>");
        }

        var html = HtmlTemplate
            .Replace(PlaceHolderTitle, directoryMovie.FullName)
            .Replace(PlaceHolderBody, stringBuilder.ToString())
            .Replace(PlaceHolderError, stringBuilderError.ToString());

        await File.WriteAllTextAsync(Path.Combine(directoryHtml.FullName, "index.html"), html, token);

        if (directoryTmp.Exists)
        {
            directoryTmp.Delete(true);
        }
    }

    private const string PlaceHolderTitle = "__TITLE__";
    private const string PlaceHolderBody = "__BODY__";
    private const string PlaceHolderError = "__ERROR__";

    private const string HtmlTemplate = """

                                        <!DOCTYPE html>
                                        <html lang="en">
                                        <head>
                                            <meta charset="UTF-8">
                                            <meta name="viewport" content="width=device-width">
                                            <title>__TITLE__</title>        
                                            <style>
                                                img {
                                                    max-width: 100%;
                                                    height: auto;
                                                }
                                            </style>
                                        </head>
                                        <body>

                                        <h2>THUMBNAIL</h2>
                                        <p>
                                            __BODY__
                                        </p>

                                        <h2>ERROR</h2>
                                        <p>
                                            __ERROR__
                                        </p>

                                        </body>
                                        </html>

                                        """;

    private const string PlaceHolderImage = "__IMG__";
    private const string PlaceHolderCaption = "__CAPTION__";

    private const string HtmlTemplateFigure = """
                                              <figure>
                                                  <figcaption>__CAPTION__</figcaption>
                                                  <img src="__IMG__">
                                              </figure>
                                              """;
}
