// dotnet run --project BlackBrownie

using BlackBrownie;
using BlackBrownie.Functions;

IFunction[] functions =
{
    new FunctionUniqueFile(),
    new FunctionExpandDirectory(),
    new FunctionDeleteDeprecated(),
    new FunctionUniqueUri(),
    new FunctionUnseal(),
    new FunctionDeleteTarget(),
    new FunctionInsertString(),
    new FunctionCompareEditorConfig(),
    new FunctionRenamePixivIndex(),
    new FunctionFilterName(),
    new FunctionDownsizeImage(),
    new FunctionZeroPadding(),
};

var cts = new CancellationTokenSource();
var token = cts.Token;
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    if (!cts.IsCancellationRequested)
    {
        Console.WriteLine("cancel!");
        cts.Cancel();
    }
};

int selectedFunctionIndex;
string[] selectedFunctionArgs;

if (args.Length > 0 && int.TryParse(args[0], out var bootIndex) && CheckIsRange(bootIndex))
{
    selectedFunctionIndex = bootIndex;
    selectedFunctionArgs = args.Skip(1).ToArray();
    var function = functions[selectedFunctionIndex];
    Console.WriteLine($"[{selectedFunctionIndex:00}] {function.DescriptionFunction()}");
}
else
{
    var (index, strings) = InteractiveMode();
    selectedFunctionIndex = index;
    selectedFunctionArgs = strings;
}

if (!CheckIsRange(selectedFunctionIndex))
{
    Console.WriteLine("out of range");
    return;
}

token.ThrowIfCancellationRequested();

var selectedFunction = functions[selectedFunctionIndex];
await selectedFunction.Do(selectedFunctionArgs, token);
return;

(int index, string[] args) InteractiveMode()
{
    for (var index = 0; index < functions.Length; index++)
    {
        var function = functions[index];
        Console.WriteLine($"[{index:00}] {function.DescriptionFunction()}");
        Console.WriteLine("---------");
    }

    Console.WriteLine("input index");

    token.ThrowIfCancellationRequested();

    var readLineIndex = Console.ReadLine();
    if (!int.TryParse(readLineIndex, out var i))
    {
        Console.WriteLine("not index");
        return (-1, []);
    }

    token.ThrowIfCancellationRequested();

    var f = functions[i];
    Console.WriteLine($"[{f.GetType().Name}] input args");
    Console.WriteLine(f.DescriptionArgs());
    var readLineArgs = Console.ReadLine();
    var argsArray = readLineArgs?.Split(new[] { ' ', '　' }, StringSplitOptions.RemoveEmptyEntries)
                    ?? Array.Empty<string>();

    return (i, argsArray);
}

bool CheckIsRange(int targetIndex)
{
    return targetIndex >= 0 && targetIndex < functions.Length;
}
