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

        var contentSpan = remainingBuffer.Slice(0, matchIndex);
        int totalLengthParsed = (currentOffset - startPos.Offset) + matchIndex + needleLength;

        for (int i = 0; i < totalLengthParsed; i++)
            cursor.Advance();

        string content;
        StringKind style;

        if (isMultiline)
        {
            content = ProcessMultiLineRawString(contentSpan);
            style = StringKind.MultiLine | StringKind.Raw;
        }
        else
        {
            content = contentSpan.ToString();
            style = StringKind.Quoted | StringKind.Raw;
        }

        result.Set(startPos.Offset, totalLengthParsed, new KdlString(content, style));
        return true;
    }

    /// <summary>
    /// Processes a multi-line raw string, handling newline normalization and dedentation.
    /// Works directly with spans to minimize allocations.
    /// </summary>
    public static string ProcessMultiLineRawString(ReadOnlySpan<char> rawInput)
    {
        if (rawInput.IsEmpty)
            return string.Empty;

        bool hasCR = rawInput.Contains('\r');

        ReadOnlySpan<char> normalized;
        string? normalizedString = null;

        if (hasCR)
        {
            normalizedString = NormalizeNewlines(rawInput);
            normalized = normalizedString.AsSpan();
        }
        else
        {
            normalized = rawInput;
        }

        int lastNewLine = normalized.LastIndexOf('\n');

        ReadOnlySpan<char> prefix;
        ReadOnlySpan<char> contentBody;

        if (lastNewLine >= 0)
        {
            prefix = normalized.Slice(lastNewLine + 1);
            contentBody = normalized.Slice(0, lastNewLine + 1);
        }
        else
        {
            prefix = normalized;
            contentBody = ReadOnlySpan<char>.Empty;
        }

        foreach (char c in prefix)
        {
            if (c != ' ' && c != '\t')
            {
                throw new ParseException(
                    "Multi-line raw string closing delimiter must be on its own line, preceded only by whitespace.",
                    TextPosition.Start
                );
            }
        }

        if (contentBody.IsEmpty)
            return string.Empty;

        int pos = 0;
        if (contentBody[0] == '\n')
            pos = 1;

        if (prefix.IsEmpty)
        {
            var body = contentBody.Slice(pos);

            if (body.Length > 0 && body[^1] == '\n')
                body = body.Slice(0, body.Length - 1);
            return body.ToString();
        }

        return BuildDedentedString(contentBody, pos, prefix);
    }

    private static string NormalizeNewlines(ReadOnlySpan<char> input)
    {
        int outputLength = 0;
        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] == '\r')
            {
                outputLength++;
                if (i + 1 < input.Length && input[i + 1] == '\n')
                    i++;
            }
            else
            {
                outputLength++;
            }
        }

        return string.Create(
            outputLength,
            input.ToString(),
            static (span, inputStr) =>
            {
                int writePos = 0;
                for (int i = 0; i < inputStr.Length; i++)
                {
                    if (inputStr[i] == '\r')
                    {
                        span[writePos++] = '\n';
                        if (i + 1 < inputStr.Length && inputStr[i + 1] == '\n')
                            i++;
                    }
                    else
                    {
                        span[writePos++] = inputStr[i];
                    }
                }
            }
        );
    }

    private static string BuildDedentedString(
        ReadOnlySpan<char> contentBody,
        int startPos,
        ReadOnlySpan<char> prefix
    )
    {
        int outputLength = 0;
        int pos = startPos;

        while (pos < contentBody.Length)
        {
            int nextNewLine = contentBody.Slice(pos).IndexOf('\n');
            if (nextNewLine < 0)
                break;

            nextNewLine += pos;
            int lineLength = nextNewLine + 1 - pos;
            var line = contentBody.Slice(pos, lineLength);

            bool isWhitespaceOnly = IsWhitespaceOnlyLine(line);

            if (isWhitespaceOnly)
            {
                outputLength++;
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
                outputLength += line.Length - prefix.Length;
            }

            pos = nextNewLine + 1;
        }

        if (outputLength > 0)
            outputLength--;

        if (outputLength <= 0)
            return string.Empty;

        string prefixStr = prefix.ToString();
        string contentStr = contentBody.ToString();

        return string.Create(
            outputLength,
            (contentStr, startPos, prefixStr),
            static (span, state) =>
            {
                var (content, startIdx, prefixS) = state;
                int writePos = 0;
                int pos = startIdx;

                while (pos < content.Length && writePos < span.Length)
                {
                    int nextNewLine = content.IndexOf('\n', pos);
                    if (nextNewLine < 0)
                        break;

                    int lineLength = nextNewLine + 1 - pos;
                    var line = content.AsSpan(pos, lineLength);

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
                        if (writePos < span.Length)
                            span[writePos++] = '\n';
                    }
                    else
                    {
                        var dedentedLine = line.Slice(prefixS.Length);
                        int copyLen = Math.Min(dedentedLine.Length, span.Length - writePos);
                        dedentedLine.Slice(0, copyLen).CopyTo(span.Slice(writePos));
                        writePos += copyLen;
                    }

                    pos = nextNewLine + 1;
                }
            }
        );
    }

    private static bool IsWhitespaceOnlyLine(ReadOnlySpan<char> line)
    {
        for (int i = 0; i < line.Length - 1; i++)
        {
            if (!char.IsWhiteSpace(line[i]))
                return false;
        }
        return true;
    }
}
