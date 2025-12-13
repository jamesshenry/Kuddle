using System;
using Kuddle.AST;
using Parlot;
using Parlot.Fluent;

namespace Kuddle.Parser;

sealed class RawStringParser : Parser<KdlString>
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

        if (quoteCount == 2)
        {
            quoteCount = 1;
            currentOffset--;
        }

        if (quoteCount != 1 && quoteCount != 3)
        {
            cursor.ResetPosition(startPos);
            return false;
        }

        bool isMultiline = quoteCount == 3;

        int needleLength = quoteCount + hashCount;
        Span<char> needle =
            needleLength <= 256 ? stackalloc char[needleLength] : new char[needleLength];

        needle.Slice(0, quoteCount).Fill('"');
        needle.Slice(quoteCount, hashCount).Fill('#');

        var remainingBuffer = bufferSpan.Slice(currentOffset);
        int matchIndex = remainingBuffer.IndexOf(needle);

        if (matchIndex < 0)
        {
            throw new ParseException(
                $"Expected raw string to be terminated with sequence '{needle.ToString()}'",
                startPos
            );
        }

        var content = remainingBuffer.Slice(0, matchIndex).ToString();

        int totalLengthParsed = (currentOffset - startPos.Offset) + matchIndex + needleLength;
        for (int i = 0; i < totalLengthParsed; i++)
            context.Scanner.Cursor.Advance();

        StringKind style;

        if (isMultiline)
        {
            content = ProcessMultiLineRawString(content);
            style = StringKind.MultiLine | StringKind.Raw;
        }
        else
        {
            style = StringKind.Quoted | StringKind.Raw;
        }

        result.Set(startPos.Offset, totalLengthParsed, new KdlString(content, style));
        return true;
    }

    public static string ProcessMultiLineRawString(string rawInput)
    {
        string text = rawInput.Replace("\r\n", "\n").Replace("\r", "\n");

        if (!text.StartsWith('\n')) { }

        int lastNewLine = text.LastIndexOf('\n');
        string prefix = "";
        string contentBody = text;

        if (lastNewLine >= 0)
        {
            prefix = text.Substring(lastNewLine + 1);
            contentBody = text.Substring(0, lastNewLine + 1);
        }
        else
        {
            prefix = text;
            contentBody = "";
        }

        foreach (char c in prefix)
        {
            if (c != ' ' && c != '\t')
                throw new ParseException(
                    "Multi-line raw string closing delimiter must be on its own line, preceded only by whitespace.",
                    TextPosition.Start
                );
        }

        var sb = new System.Text.StringBuilder();
        int pos = 0;

        if (contentBody.StartsWith('\n'))
        {
            pos = 1;
        }

        while (pos < contentBody.Length)
        {
            int nextNewLine = contentBody.IndexOf('\n', pos);
            if (nextNewLine == -1)
                break;

            int lineLength = (nextNewLine + 1) - pos;
            ReadOnlySpan<char> line = contentBody.AsSpan(pos, lineLength);

            bool isWhitespaceOnly = true;
            for (int i = 0; i < line.Length - 1; i++)
            {
                if (!char.IsWhiteSpace(line[i]))
                {
                    isWhitespaceOnly = false;
                    break;
                }
            }

            if (isWhitespaceOnly)
            {
                sb.Append('\n');
            }
            else
            {
                if (!line.StartsWith(prefix))
                {
                    throw new ParseException(
                        "Multi-line string indentation error: Line does not match closing delimiter indentation.",
                        TextPosition.Start
                    );
                }

                sb.Append(line.Slice(prefix.Length));
            }

            pos = nextNewLine + 1;
        }

        string result = sb.ToString();

        if (result.EndsWith('\n'))
        {
            result = result.Substring(0, result.Length - 1);
        }

        return result;
    }
}
