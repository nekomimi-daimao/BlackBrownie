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
};

for (var index = 0; index < functions.Length; index++)
{
    var function = functions[index];
    Console.WriteLine($"[{index:00}] {function.DescriptionFunction()}");
    Console.WriteLine("---------");
}

Console.WriteLine("input index");
var readLineIndex = Console.ReadLine();
if (!int.TryParse(readLineIndex, out var i))
{
    Console.WriteLine("not index");
    return;
}

if (i < 0 || i >= functions.Length)
{
    Console.WriteLine("out of range");
    return;
}

var f = functions[i];
Console.WriteLine($"[{f.GetType().Name}] input args");
Console.WriteLine(f.DescriptionArgs());
var readLineArgs = Console.ReadLine();
var argsArray = readLineArgs?.Split(new[] { ' ', '　' }, StringSplitOptions.RemoveEmptyEntries)
                ?? Array.Empty<string>();
await f.Do(argsArray);
