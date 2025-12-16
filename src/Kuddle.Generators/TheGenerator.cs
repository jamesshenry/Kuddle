using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kuddle.Generators;

[Generator]
public class TheGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context
            .SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, _) => ctx.Node as ClassDeclarationSyntax
            )
            .Where(m => m is not null);

        var compilation = context.CompilationProvider.Combine(provider.Collect());

        context.RegisterSourceOutput(compilation, Execute);
    }

    private void Execute(
        SourceProductionContext context,
        (Compilation Left, ImmutableArray<ClassDeclarationSyntax?> Right) tuple
    )
    {
        var (compilation, list) = tuple;

        var nameList = new List<string>();

        foreach (var syntax in list)
        {
            var symbol =
                compilation.GetSemanticModel(syntax.SyntaxTree).GetDeclaredSymbol(syntax)
                as INamedTypeSymbol;

            nameList.Add(
                $"""
                "{symbol.ToDisplayString()}"
                """
            );
        }

        var names = string.Join(",", nameList);

        var theCode = $$"""
namespace ClassListGenerator;

public static class ClassNames
{
    public static List<string> Names = new()
    {
        {{names}}
    };
}
""";

        context.AddSource("YourClassList.g.cs", theCode);
    }
}
