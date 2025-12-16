using System.Reflection;
using static System.Console;

// var classTypes = Assembly
//     .GetEntryAssembly()
//     ?.GetTypes()
//     .Where(t => t.IsClass && t.IsPublic)
//     .ToList();

if (ClassListGenerator.ClassNames.Names is not null)
{
    foreach (var c in ClassListGenerator.ClassNames.Names)
    {
        WriteLine(c);
    }
}
