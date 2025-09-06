
using System.Text.RegularExpressions;

namespace BlackBrownie.Functions;

public class FunctionZeroPadding : IFunction
{
    public string DescriptionFunction() => "Pads numbers in filenames with zeros and copies them to another directory.";

    public string DescriptionArgs() => "[input directory path] [output directory path] [padding digits]";

    public async Task Do(string[] args, CancellationToken token)
    {
        if (args.Length < 3)
        {
            Console.WriteLine($"Not enough arguments. {DescriptionArgs()}");
            return;
        }

        var inputDirectory = args[0];
        var outputDirectory = args[1];
        if (!int.TryParse(args[2], out var padding))
        {
            Console.WriteLine("Invalid padding digits.");
            return;
        }

        if (!Directory.Exists(inputDirectory))
        {
            Console.WriteLine("Input directory does not exist.");
            return;
        }

        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
            Console.WriteLine($"created directory: {outputDirectory}");
        }

        var files = Directory.GetFiles(inputDirectory);
        var regex = new Regex(@"(?<prefix>.*_)(?<number>\d+)(?<extension>\..*)");

        foreach (var file in files)
        {
            token.ThrowIfCancellationRequested();
            var fileName = Path.GetFileName(file);
            var match = regex.Match(fileName);

            if (!match.Success) continue;

            var prefix = match.Groups["prefix"].Value;
            var number = match.Groups["number"].Value;
            var extension = match.Groups["extension"].Value;

            var newFileName = $"{prefix}{number.PadLeft(padding, '0')}{extension}";
            var newFilePath = Path.Combine(outputDirectory, newFileName);

            Console.WriteLine($"{file}");
            Console.WriteLine($"{newFilePath}");
            File.Copy(file, newFilePath, true);
        }
        await Task.CompletedTask;
    }
}
