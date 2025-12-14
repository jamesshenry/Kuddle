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
    private static readonly Parser<KdlDocument> _parser = KuddleGrammar.Document.Compile();

    /// <summary>
    /// Parses a KDL string into a KdlDocument AST.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    /// <exception cref="KuddleParseException"></exception>
    public static KdlDocument Parse(string text, KuddleReaderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(text);
        options ??= KuddleReaderOptions.Default;

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
            KuddleReservedTypeValidator.Validate(doc);
        }

        return doc;
    }

    // /// <summary>
    // /// Reads a stream assuming UTF-8 encoding.
    // /// </summary>
    // public static async Task<KdlDocument> ReadAsync(
    //     Stream stream,
    //     KuddleReaderOptions? options = null
    // )
    // {
    //     using var reader = new StreamReader(stream);
    //     var text = await reader.ReadToEndAsync();
    //     return await ReadASync(text, options);
    // }

    public static async Task<KdlDocument> ReadAsync(
        string text,
        KuddleReaderOptions? options = null
    )
    {
        return Parse(text, options);
    }
}
