using System;
using System.Text;
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

            int backslashCount = 0;
            int backScan = matchIndex - 1;

            while (backScan >= searchOffset && searchSpan[backScan] == '\\')
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
        string text = rawInput.ToString().Replace("\r\n", "\n").Replace("\r", "\n");

        text = ResolveWsEscapes(text);

        int lastNewLine = text.LastIndexOf('\n');

        string prefix;
        string contentBody;

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
            if (!CharacterSets.IsWhiteSpace(c))
                throw new ParseException(
                    "Multi-line string closing delimiter must be on its own line.",
                    TextPosition.Start
                );
        }

        var sb = new StringBuilder();
        int pos = 0;

        if (contentBody.StartsWith('\n'))
            pos = 1;

        while (pos < contentBody.Length)
        {
            int nextNewLine = contentBody.IndexOf('\n', pos);
            if (nextNewLine == -1)
                break;

            int lineLength = nextNewLine + 1 - pos;
            var line = contentBody.AsSpan(pos, lineLength);

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
                sb.Append('\n');
            }
            else
            {
                if (!line.StartsWith(prefix))
                    throw new ParseException(
                        "Multi-line string indentation mismatch.",
                        context.Scanner.Cursor.Position
                    );

                sb.Append(line.Slice(prefix.Length));
            }

            pos = nextNewLine + 1;
        }

        string dedented = sb.ToString();

        if (dedented.EndsWith('\n'))
            dedented = dedented.Substring(0, dedented.Length - 1);

        string finalValue = UnescapeStandardKdl(dedented);

        return new KdlString(finalValue, StringKind.MultiLine);
    }

    private static string ResolveWsEscapes(string input)
    {
        if (input.IndexOf('\\') == -1)
            return input;

        var sb = new StringBuilder(input.Length);
        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            if (c == '\\')
            {
                int scanIdx = i + 1;
                bool isWsEscape = false;

                while (scanIdx < input.Length)
                {
                    char next = input[scanIdx];
                    if (next == ' ' || next == '\t')
                    {
                        scanIdx++;
                        continue;
                    }
                    break;
                }

                if (scanIdx < input.Length && input[scanIdx] == '\n')
                {
                    isWsEscape = true;
                    scanIdx++;
                    while (scanIdx < input.Length)
                    {
                        char next = input[scanIdx];
                        if (next == ' ' || next == '\t')
                        {
                            scanIdx++;
                            continue;
                        }
                        break;
                    }

                    i = scanIdx - 1;
                }
                else if (scanIdx >= input.Length)
                {
                    isWsEscape = true;
                    i = scanIdx - 1;
                }

                if (isWsEscape)
                    continue;
            }
            sb.Append(c);
        }
        return sb.ToString();
    }

    private static string UnescapeStandardKdl(string input)
    {
        var sb = new StringBuilder(input.Length);
        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            if (c == '\\' && i + 1 < input.Length)
            {
                switch (input[i + 1])
                {
                    case 'n':
                        sb.Append('\n');
                        i++;
                        break;
                    case 'r':
                        sb.Append('\r');
                        i++;
                        break;
                    case 't':
                        sb.Append('\t');
                        i++;
                        break;
                    case '\\':
                        sb.Append('\\');
                        i++;
                        break;
                    case '"':
                        sb.Append('"');
                        i++;
                        break;
                    case 'b':
                        sb.Append('\b');
                        i++;
                        break;
                    case 'f':
                        sb.Append('\f');
                        i++;
                        break;
                    case '/':
                        sb.Append('/');
                        i++;
                        break;
                    case 's':
                        sb.Append(' ');
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
                                    sb.Append(char.ConvertFromUtf32(codePoint));
                                    i = endBrace;
                                }
                                catch
                                {
                                    sb.Append(c);
                                }
                            }
                            else
                            {
                                sb.Append(c);
                            }
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }
}
