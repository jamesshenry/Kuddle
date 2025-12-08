using System;
using Kuddle.AST;
using Kuddle.Exceptions;
using Kuddle.Parser;
using Parlot.Fluent;

namespace Kuddle;

public class KdlParser
{
    private readonly Parser<KdlDocument> V2 = KuddleGrammar.Document.Compile();

    public KdlDocument Parse(string text, KuddleOptions? options = null)
    {
        options ??= KuddleOptions.Default;
        try
        {
            return V2.Parse(text)!;
        }
        catch (Exception ex)
        {
            throw new KdlParseException(ex);
        }
    }
}

public record KuddleOptions
{
    public static KuddleOptions Default => new() { ValidateReservedTypes = true };
    public bool ValidateReservedTypes { get; init; } = true;
}
