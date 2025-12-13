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

        // 1. Scan Hash Delimiters
        int hashCount = 0;
        while (currentOffset < bufferSpan.Length && bufferSpan[currentOffset] == '#')
        {
            hashCount++;
            currentOffset++;
        }

        // 2. Scan Quote Delimiters
        int quoteCount = 0;
        while (currentOffset < bufferSpan.Length && bufferSpan[currentOffset] == '"')
        {
            quoteCount++;
            currentOffset++;
        }

        // KDL only supports 1 quote (#") or 3 quotes (#""")
        // If we found 2, it's likely an empty string #""# (treated as 1 quote start/end)
        if (quoteCount == 2)
        {
            quoteCount = 1;
            currentOffset--; // Backtrack one char so we don't consume the closing quote yet
        }

        if (quoteCount != 1 && quoteCount != 3)
        {
            // Invalid syntax
            cursor.ResetPosition(startPos);
            return false;
        }

        bool isMultiline = quoteCount == 3;

        // 3. Prepare Needle (Quotes + Hashes)
        int needleLength = quoteCount + hashCount;
        Span<char> needle =
            needleLength <= 256 ? stackalloc char[needleLength] : new char[needleLength];

        needle.Slice(0, quoteCount).Fill('"');
        needle.Slice(quoteCount, hashCount).Fill('#');

        // 4. Search Content
        var remainingBuffer = bufferSpan.Slice(currentOffset);
        int matchIndex = remainingBuffer.IndexOf(needle);

        if (matchIndex < 0)
        {
            throw new ParseException(
                $"Expected raw string to be terminated with sequence '{needle.ToString()}'",
                startPos
            );
        }

        // Extract raw content
        var content = remainingBuffer.Slice(0, matchIndex).ToString();

        // 5. Advance Cursor
        // matchIndex is relative to currentOffset. Add needleLength to consume delimiter.
        int totalLengthParsed = (currentOffset - startPos.Offset) + matchIndex + needleLength;
        for (int i = 0; i < totalLengthParsed; i++)
            context.Scanner.Cursor.Advance();

        // 6. Post-Process
        StringKind style;

        if (isMultiline)
        {
            // Raw Multiline must follow dedent rules, but NO escaping
            content = ProcessMultiLineRawString(content);
            style = StringKind.MultiLine | StringKind.Raw;
        }
        else
        {
            // Raw Single line is taken literally
            style = StringKind.Quoted | StringKind.Raw;
        }

        result.Set(startPos.Offset, totalLengthParsed, new KdlString(content, style));
        return true;
    }

    // Place this inside KuddleGrammar class
    public static string ProcessMultiLineRawString(string rawInput)
    {
        // 1. Normalize Newlines
        string text = rawInput.Replace("\r\n", "\n").Replace("\r", "\n");

        // 2. Check Strict KDL Requirement: Must start with a Newline
        if (!text.StartsWith('\n'))
        {
            // For robustness, we might allow it, but Spec says "MUST immediately start with a Newline"
            // If your parser didn't capture the newline, handle that here.
            // Based on the parser above, 'content' includes the first newline if present.
        }

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

        // Skip the VERY FIRST newline (KDL Spec: "omits the first and last Newline")
        if (contentBody.StartsWith('\n'))
        {
            pos = 1;
        }

        while (pos < contentBody.Length)
        {
            int nextNewLine = contentBody.IndexOf('\n', pos);
            if (nextNewLine == -1)
                break; // Should not happen given logic above

            // Extract line (including the \n)
            int lineLength = (nextNewLine + 1) - pos;
            ReadOnlySpan<char> line = contentBody.AsSpan(pos, lineLength);

            // Check if line is whitespace-only (excluding the trailing \n)
            // KDL Spec: "Whitespace-only lines... always represent empty lines... regardless of what whitespace they contain"
            // This handles your unicode space case: "    " -> Empty Line
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
                // Keep the newline, ignore the horizontal content
                sb.Append('\n');
            }
            else
            {
                // Check Indentation
                if (!line.StartsWith(prefix))
                {
                    throw new ParseException(
                        "Multi-line string indentation error: Line does not match closing delimiter indentation.",
                        TextPosition.Start
                    );
                }

                // Append Dedented Line
                sb.Append(line.Slice(prefix.Length));
            }

            pos = nextNewLine + 1;
        }

        string result = sb.ToString();

        // 6. Remove Final Newline
        // The loop above preserves the newline of the last content line.
        // KDL Spec says to omit the last newline (the one before the closing quotes).
        if (result.EndsWith('\n'))
        {
            result = result.Substring(0, result.Length - 1);
        }

        return result;
    }
}
