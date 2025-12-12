using System;
using Kuddle.AST;
using Kuddle.Parser;
using Parlot;
using Parlot.Fluent;

public class RawStringParser : Parser<KdlString>
{
    public override bool Parse(ParseContext context, ref ParseResult<KdlString> result)
    {
        var cursor = context.Scanner.Cursor;
        var bufferSpan = context.Scanner.Buffer.AsSpan();
        if (cursor.Current != '#')
            return false;

        var startPos = cursor.Position;
        int currentOffset = startPos.Offset;

        int hashCount = 0;
        while (currentOffset < bufferSpan.Length && bufferSpan[currentOffset] == '#')
        {
            hashCount++;
            currentOffset++;
        }

        int quoteCount = 0;
        while (currentOffset < bufferSpan.Length && bufferSpan[currentOffset] == '"')
        {
            quoteCount++;
            currentOffset++;
        }

        // KDL only supports " or """
        if (quoteCount > 3 || quoteCount < 1)
        {
            // Reset and fail (or throw error if you want strictness here)
            // But usually we return false to let other parsers try.
            cursor.ResetPosition(startPos);
            return false;
        }
        if (quoteCount == 2)
        {
            quoteCount--;
            currentOffset--;
        }

        bool isMultiline = quoteCount == 3;

        int needleLength = quoteCount + hashCount;
        Span<char> needle =
            needleLength <= 256 ? stackalloc char[needleLength] : new char[needleLength];

        needle[..quoteCount].Fill('"');
        // Fill hashes
        needle.Slice(quoteCount, hashCount).Fill('#');

        var remainingBuffer = bufferSpan.Slice(currentOffset);

        int matchIndex = remainingBuffer.IndexOf(needle);

        if (matchIndex < 0)
        {
            throw new ParseException(
                $"Expected raw string to be terminated with {needle}",
                startPos
            );
        }

        var content = remainingBuffer.Slice(0, matchIndex).ToString();

        int totalLengthParsed = currentOffset - startPos.Offset + matchIndex + needleLength;

        // Advance the Parlot cursor
        // Note: Parlot's cursor doesn't have an "Advance(int count)" easily accessible
        // without a loop unless we manipulate the offset directly, but usually we do:
        for (int i = 0; i < totalLengthParsed; i++)
            context.Scanner.Cursor.Advance();

        // 8. Process Flags/Dedenting
        if (isMultiline)
        {
            content = KuddleGrammar.Dedent(content);
        }

        var style = isMultiline
            ? (StringKind.MultiLine | StringKind.Raw)
            : (StringKind.Quoted | StringKind.Raw);

        result.Set(startPos.Offset, totalLengthParsed, new KdlString(content, style));
        return true;

        throw new ParseException("Unterminated raw string", startPos);
    }
}
