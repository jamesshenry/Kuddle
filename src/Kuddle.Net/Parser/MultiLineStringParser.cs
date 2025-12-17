using System;
using Kuddle.AST;
using Parlot;
using Parlot.Fluent;

namespace Kuddle.Parser;

public class MultiLineStringParser : Parser<KdlString>
{
    public override bool Parse(ParseContext context, ref ParseResult<KdlString> result)
    {
        var cursor = context.Scanner.Cursor;

        if (!cursor.Match("\"\"\""))
            return false;

        var startPos = cursor.Position;

        cursor.Advance();
        cursor.Advance();
        cursor.Advance();

        bool hasNewline = false;

        if (cursor.Match("\r\n"))
        {
            cursor.Advance();
            cursor.Advance();
            hasNewline = true;
        }
        else if (cursor.Current == '\n')
        {
            cursor.Advance();
            hasNewline = true;
        }

        if (!hasNewline)
        {
            throw new ParseException(
                "Multi-line strings must start with a newline immediately after the opening \"\"\".",
                cursor.Position
            );
        }

        var searchSpan = context.Scanner.Buffer.AsSpan(cursor.Position.Offset);

        int searchOffset = 0;

        while (true)
        {
            int relativeIndex = searchSpan.Slice(searchOffset).IndexOf("\"\"\"");

            if (relativeIndex < 0)
            {
                throw new ParseException("Unterminated multi-line string.", startPos);
            }

            int matchIndex = searchOffset + relativeIndex;

            // Count preceding backslashes
            int backslashCount = 0;
            int backScan = matchIndex - 1;

            while (backScan >= 0 && searchSpan[backScan] == '\\')
            {
                backslashCount++;
                backScan--;
            }

            if (backslashCount % 2 == 0)
            {
                var contentSpan = searchSpan.Slice(0, matchIndex);

                int charsToAdvance = matchIndex + 3;
                for (int i = 0; i < charsToAdvance; i++)
                    cursor.Advance();

                KdlString kdlString = ProcessMultiLineString(contentSpan, context);

                result.Set(startPos.Offset, cursor.Position.Offset - startPos.Offset, kdlString);
                return true;
            }

            searchOffset = matchIndex + 1;
        }
    }

    private static KdlString ProcessMultiLineString(
        ReadOnlySpan<char> rawInput,
        ParseContext context
    )
    {
        // Fast path: empty content
        if (rawInput.IsEmpty)
            return new KdlString(string.Empty, StringKind.MultiLine);

        // Check what processing we need
        bool hasCR = rawInput.Contains('\r');
        bool hasBackslash = rawInput.Contains('\\');

        // If no special characters, we can take a faster path
        ReadOnlySpan<char> normalized;
        string? workingString = null;

        if (hasCR)
        {
            workingString = NormalizeNewlines(rawInput);
            normalized = workingString.AsSpan();
        }
        else
        {
            normalized = rawInput;
        }

        // Handle whitespace escapes if present
        if (hasBackslash)
        {
            workingString = ResolveWsEscapes(workingString ?? normalized.ToString());
            normalized = workingString.AsSpan();
        }

        // Find the last newline to extract the prefix (indentation)
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

        // Validate prefix contains only whitespace
        foreach (char c in prefix)
        {
            if (!CharacterSets.IsWhiteSpace(c))
            {
                throw new ParseException(
                    "Multi-line string closing delimiter must be on its own line.",
                    TextPosition.Start
                );
            }
        }

        // Build dedented string
        string dedented = BuildDedentedString(contentBody, prefix, context);

        // Unescape if needed
        string finalValue = hasBackslash ? UnescapeStandardKdl(dedented) : dedented;

        return new KdlString(finalValue, StringKind.MultiLine);
    }

    private static string NormalizeNewlines(ReadOnlySpan<char> input)
    {
        // Count output size first
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
        ReadOnlySpan<char> prefix,
        ParseContext context
    )
    {
        if (contentBody.IsEmpty)
            return string.Empty;

        // Skip leading newline
        int startPos = 0;
        if (contentBody[0] == '\n')
            startPos = 1;

        // Fast path: no prefix means no dedentation needed
        if (prefix.IsEmpty)
        {
            var body = contentBody.Slice(startPos);
            if (body.Length > 0 && body[^1] == '\n')
                body = body.Slice(0, body.Length - 1);
            return body.ToString();
        }

        // First pass: calculate output length and validate
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
                        "Multi-line string indentation mismatch.",
                        context.Scanner.Cursor.Position
                    );
                }
                outputLength += line.Length - prefix.Length;
            }

            pos = nextNewLine + 1;
        }

        // Remove trailing newline
        if (outputLength > 0)
            outputLength--;

        if (outputLength <= 0)
            return string.Empty;

        // Second pass: build string
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
                        if (!CharacterSets.IsWhiteSpace(line[i]))
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
            if (!CharacterSets.IsWhiteSpace(line[i]))
                return false;
        }
        return true;
    }

    private static string ResolveWsEscapes(string input)
    {
        // Fast path: no backslashes
        int backslashIdx = input.IndexOf('\\');
        if (backslashIdx == -1)
            return input;

        // Check if any backslash is followed by whitespace/newline (ws-escape pattern)
        bool hasWsEscape = false;
        for (int i = backslashIdx; i < input.Length - 1; i++)
        {
            if (input[i] == '\\')
            {
                char next = input[i + 1];
                if (next == ' ' || next == '\t' || next == '\n')
                {
                    hasWsEscape = true;
                    break;
                }
            }
        }

        if (!hasWsEscape)
            return input;

        // Need to process ws-escapes
        // First pass: calculate output length
        int outputLength = 0;
        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] == '\\')
            {
                int scanIdx = i + 1;

                // Skip whitespace
                while (scanIdx < input.Length && (input[scanIdx] == ' ' || input[scanIdx] == '\t'))
                    scanIdx++;

                // Check for newline
                if (scanIdx < input.Length && input[scanIdx] == '\n')
                {
                    scanIdx++;
                    // Skip whitespace after newline
                    while (
                        scanIdx < input.Length && (input[scanIdx] == ' ' || input[scanIdx] == '\t')
                    )
                        scanIdx++;

                    i = scanIdx - 1;
                    continue;
                }
                else if (scanIdx >= input.Length && scanIdx > i + 1)
                {
                    // Trailing ws-escape
                    i = scanIdx - 1;
                    continue;
                }
            }
            outputLength++;
        }

        return string.Create(
            outputLength,
            input,
            static (span, inp) =>
            {
                int writePos = 0;
                for (int i = 0; i < inp.Length; i++)
                {
                    if (inp[i] == '\\')
                    {
                        int scanIdx = i + 1;

                        while (
                            scanIdx < inp.Length && (inp[scanIdx] == ' ' || inp[scanIdx] == '\t')
                        )
                            scanIdx++;

                        if (scanIdx < inp.Length && inp[scanIdx] == '\n')
                        {
                            scanIdx++;
                            while (
                                scanIdx < inp.Length
                                && (inp[scanIdx] == ' ' || inp[scanIdx] == '\t')
                            )
                                scanIdx++;

                            i = scanIdx - 1;
                            continue;
                        }
                        else if (scanIdx >= inp.Length && scanIdx > i + 1)
                        {
                            i = scanIdx - 1;
                            continue;
                        }
                    }
                    span[writePos++] = inp[i];
                }
            }
        );
    }

    private static string UnescapeStandardKdl(string input)
    {
        // Fast path: no backslashes
        int backslashIdx = input.IndexOf('\\');
        if (backslashIdx == -1)
            return input;

        // First pass: calculate output length
        int outputLength = 0;
        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] == '\\' && i + 1 < input.Length)
            {
                char next = input[i + 1];
                switch (next)
                {
                    case 'n':
                    case 'r':
                    case 't':
                    case '\\':
                    case '"':
                    case 'b':
                    case 'f':
                    case '/':
                    case 's':
                        outputLength++;
                        i++;
                        break;
                    case ' ':
                        i++;
                        break;
                    case 'u':
                        if (i + 2 < input.Length && input[i + 2] == '{')
                        {
                            int endBrace = input.IndexOf('}', i + 3);
                            if (endBrace > i + 2)
                            {
                                string hex = input.Substring(i + 3, endBrace - (i + 3));
                                try
                                {
                                    int codePoint = Convert.ToInt32(hex, 16);
                                    outputLength += char.ConvertFromUtf32(codePoint).Length;
                                    i = endBrace;
                                }
                                catch
                                {
                                    outputLength++;
                                }
                            }
                            else
                            {
                                outputLength++;
                            }
                        }
                        else
                        {
                            outputLength++;
                        }
                        break;
                    default:
                        outputLength++;
                        break;
                }
            }
            else
            {
                outputLength++;
            }
        }

        return string.Create(
            outputLength,
            input,
            static (span, inp) =>
            {
                int writePos = 0;
                for (int i = 0; i < inp.Length; i++)
                {
                    if (inp[i] == '\\' && i + 1 < inp.Length)
                    {
                        char next = inp[i + 1];
                        switch (next)
                        {
                            case 'n':
                                span[writePos++] = '\n';
                                i++;
                                break;
                            case 'r':
                                span[writePos++] = '\r';
                                i++;
                                break;
                            case 't':
                                span[writePos++] = '\t';
                                i++;
                                break;
                            case '\\':
                                span[writePos++] = '\\';
                                i++;
                                break;
                            case '"':
                                span[writePos++] = '"';
                                i++;
                                break;
                            case 'b':
                                span[writePos++] = '\b';
                                i++;
                                break;
                            case 'f':
                                span[writePos++] = '\f';
                                i++;
                                break;
                            case '/':
                                span[writePos++] = '/';
                                i++;
                                break;
                            case 's':
                                span[writePos++] = ' ';
                                i++;
                                break;
                            case ' ':
                                i++;
                                break;
                            case 'u':
                                if (i + 2 < inp.Length && inp[i + 2] == '{')
                                {
                                    int endBrace = inp.IndexOf('}', i + 3);
                                    if (endBrace > i + 2)
                                    {
                                        string hex = inp.Substring(i + 3, endBrace - (i + 3));
                                        try
                                        {
                                            int codePoint = Convert.ToInt32(hex, 16);
                                            string utf32 = char.ConvertFromUtf32(codePoint);
                                            utf32.AsSpan().CopyTo(span.Slice(writePos));
                                            writePos += utf32.Length;
                                            i = endBrace;
                                        }
                                        catch
                                        {
                                            span[writePos++] = inp[i];
                                        }
                                    }
                                    else
                                    {
                                        span[writePos++] = inp[i];
                                    }
                                }
                                else
                                {
                                    span[writePos++] = inp[i];
                                }
                                break;
                            default:
                                span[writePos++] = inp[i];
                                break;
                        }
                    }
                    else
                    {
                        span[writePos++] = inp[i];
                    }
                }
            }
        );
    }
}
