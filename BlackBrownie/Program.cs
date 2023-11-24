using BlackBrownie;
using BlackBrownie.Functions;

IFunction[] functions =
{
    new FunctionDeleteDeprecated(),
};

for (var index = 0; index < functions.Length; index++)
{
    var function = functions[index];
    Console.WriteLine(index);
    Console.WriteLine(function.DescriptionFunction());
    Console.WriteLine("---------");
}

