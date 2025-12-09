using System;
using Kuddle.AST;
using Kuddle.Exceptions;
using Kuddle.Parser;
using Kuddle.Validation;
using Microsoft.VisualBasic;
using Parlot.Fluent;

namespace Kuddle;

public class KdlParser
{
    private readonly Parser<KdlDocument> V2 = KuddleGrammar.Document.Compile();

    public KdlDocument Parse(string text, KuddleOptions? options = null)
    {
        options ??= KuddleOptions.Default;
        KdlDocument doc;
        if (!V2.TryParse(text, out doc!, out var error))
        {
            throw new KdlParseException("Parsing failed");
        }

        if (options.ValidateReservedTypes)
        {
            KdlReservedTypeValidator.Validate(doc);
        }

        return doc;
    }
}

public record KuddleOptions
{
    public static KuddleOptions Default => new() { ValidateReservedTypes = true };
    public bool ValidateReservedTypes { get; init; } = true;
}
