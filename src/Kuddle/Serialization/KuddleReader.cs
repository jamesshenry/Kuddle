using System;
using System.IO;
using System.Threading.Tasks;
using Kuddle.AST;
using Kuddle.Exceptions;
using Kuddle.Parser;
using Kuddle.Validation;
using Parlot.Fluent;

namespace Kuddle;

public static class KuddleReader
{
    private static readonly Parser<KdlDocument> _parser = KdlGrammar.Document.Compile();

    /// <summary>
    /// Parses a KDL string into a KdlDocument AST.
    /// </summary>
    public static KdlDocument Parse(string text, KuddleReaderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(text);
        options ??= KuddleReaderOptions.Default;

        if (!_parser.TryParse(text, out var doc, out var error))
        {
            if (error != null)
            {
                throw new KdlParseException(
                    error.Message,
                    error.Position.Column,
                    error.Position.Line,
                    error.Position.Offset
                );
            }
            throw new KdlParseException("Parsing failed unexpectedly.");
        }

        if (options.ValidateReservedTypes)
        {
            KdlReservedTypeValidator.Validate(doc);
        }

        return doc;
    }

    /// <summary>
    /// Reads a stream assuming UTF-8 encoding.
    /// </summary>
    public static async Task<KdlDocument> ParseAsync(
        Stream stream,
        KuddleReaderOptions? options = null
    )
    {
        using var reader = new StreamReader(stream);
        var text = await reader.ReadToEndAsync();
        return Parse(text, options);
    }
}
