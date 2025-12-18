using System;
using Kuddle.AST;
using Kuddle.Exceptions;
using Kuddle.Parser;
using Kuddle.Validation;
using Parlot.Fluent;

namespace Kuddle.Serialization;

public static class KdlReader
{
    private static readonly Parser<KdlDocument> _parser = KdlGrammar.Document.Compile();

    /// <summary>
    /// Parses a KDL string into a KdlDocument AST.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    /// <exception cref="KuddleParseException"></exception>
    public static KdlDocument Read(string text, KdlReaderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(text);
        options ??= KdlReaderOptions.Default;

        if (!_parser.TryParse(text, out var doc, out var error))
        {
            if (error != null)
            {
                throw new KuddleParseException(
                    error.Message,
                    error.Position.Column,
                    error.Position.Line,
                    error.Position.Offset
                );
            }
            throw new KuddleParseException("Parsing failed unexpectedly.");
        }

        if (options.ValidateReservedTypes)
        {
            KdlReservedTypeValidator.Validate(doc);
        }

        return doc;
    }
}
