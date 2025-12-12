using System;
using Parlot;
using Parlot.Fluent;

namespace Kuddle.Parser;

public class DebugParser<T> : Parser<T>
{
    private readonly Parser<T> _inner;
    private readonly string _name;

    public DebugParser(Parser<T> inner, string name)
    {
        _inner = inner;
        _name = name;
    }

    public override bool Parse(ParseContext context, ref ParseResult<T> result)
    {
        var startCursor = context.Scanner.Cursor.Position;

        // Peek at the next few chars to see what we are looking at
        var peekPreview = context
            .Scanner.Buffer.Substring(
                startCursor.Offset,
                Math.Min(10, context.Scanner.Buffer.Length - startCursor.Offset)
            )
            .Replace("\n", "\\n")
            .Replace("\r", "\\r");

        System.Diagnostics.Debug.WriteLine(
            $"[START] {_name} at {startCursor.Line}:{startCursor.Column} (Input: '{peekPreview}...')"
        );

        if (_inner.Parse(context, ref result))
        {
            System.Diagnostics.Debug.WriteLine($"[MATCH] {_name} -> {result.Value}");
            return true;
        }

        System.Diagnostics.Debug.WriteLine($"[FAIL ] {_name}");
        return false;
    }
}
