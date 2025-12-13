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

        // Consume opening """
        cursor.Advance();
        cursor.Advance();
        cursor.Advance();

        // 2. Mandatory Newline check
        // KDL Spec: "Its first line MUST immediately start with a Newline"
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
            // We have consumed """, so failing to find a newline is a syntax error, not a mismatch.
            throw new ParseException(
                "Multi-line strings must start with a newline immediately after the opening \"\"\".",
                cursor.Position
            );
        }

        var searchSpan = context.Scanner.Buffer.AsSpan(cursor.Position.Offset);

        int searchOffset = 0;

        while (true)
        {
            // Fast vectorised search
            int relativeIndex = searchSpan.Slice(searchOffset).IndexOf("\"\"\"");

            if (relativeIndex < 0)
            {
                throw new ParseException("Unterminated multi-line string.", startPos);
            }

            int matchIndex = searchOffset + relativeIndex;

            // 4. Verify Delimiter is not escaped
            // Count consecutive backslashes immediately preceding the match
            int backslashCount = 0;
            int backScan = matchIndex - 1;

            while (backScan >= searchOffset && searchSpan[backScan] == '\\')
            {
                backslashCount++;
                backScan--;
            }

            // Even number of backslashes means the backslashes escape each other,
            // and the quotes are active.
            // Odd number (e.g. \") means the quote is escaped.
            if (backslashCount % 2 == 0)
            {
                // Found valid closing delimiter!

                // Extract raw content (excluding start quotes/newline and end quotes)
                var contentSpan = searchSpan.Slice(0, matchIndex);

                // Advance the cursor past content + closing delimiter (3 chars)
                // Note: Cursor was left at contentStartOffset
                int charsToAdvance = matchIndex + 3;
                for (int i = 0; i < charsToAdvance; i++)
                    cursor.Advance();

                // 5. Post-Process (Dedent, Unescape)
                // Using the helper method we defined in KuddleGrammar
                // Note: Ensure ProcessMultiLineString is 'internal' so we can access it here
                KdlString kdlString = ProcessMultiLineString(contentSpan, context);

                result.Set(startPos.Offset, cursor.Position.Offset - startPos.Offset, kdlString);
                return true;
            }

            // If escaped (e.g. \""" ), skip this occurrence and continue searching
            searchOffset = matchIndex + 1;
        }
    }

    private static KdlString ProcessMultiLineString(
        ReadOnlySpan<char> rawInput,
        ParseContext context
    )
    {
        // 1. Normalize Newlines
        string text = rawInput.ToString().Replace("\r\n", "\n").Replace("\r", "\n");

        // 2. Resolve Whitespace Escapes (MUST happen before Dedent)
        // This merges lines like "foo \ \n bar" into "foo bar"
        text = ResolveWsEscapes(text);

        // 3. Find Indentation (Prefix) from the last line
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

        // Validate Prefix (Must be whitespace only)
        foreach (char c in prefix)
        {
            if (!CharacterSets.IsWhiteSpace(c))
                throw new ParseException(
                    "Multi-line string closing delimiter must be on its own line.",
                    TextPosition.Start
                );
        }

        // 4. Dedent
        var sb = new StringBuilder();
        int pos = 0;

        // Skip the very first newline (KDL spec: first/last newlines omitted)
        if (contentBody.StartsWith('\n'))
            pos = 1;

        while (pos < contentBody.Length)
        {
            int nextNewLine = contentBody.IndexOf('\n', pos);
            if (nextNewLine == -1)
                break;

            int lineLength = nextNewLine + 1 - pos;
            var line = contentBody.AsSpan(pos, lineLength);

            // Check if line is whitespace-only (excluding the trailing \n)
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
                sb.Append('\n'); // Preserve empty lines
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

        // Remove the final newline (the one before the closing quotes)
        if (dedented.EndsWith('\n'))
            dedented = dedented.Substring(0, dedented.Length - 1);

        // 5. Unescape Standard Characters (\n, \t, \\, etc.)
        // Note: WS escapes are already gone.
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

                // 1. Consume whitespace on the CURRENT line (before the newline)
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

                // 2. Check for Newline or EOF
                if (scanIdx < input.Length && input[scanIdx] == '\n')
                {
                    // Found '\' + ws* + '\n'
                    isWsEscape = true;
                    scanIdx++; // Consume the newline

                    // 3. Consume whitespace on the NEXT line (leading indentation)
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

                    // Advance main loop to the character after the consumed sequence
                    // (minus 1 because the loop performs i++)
                    i = scanIdx - 1;
                }
                else if (scanIdx >= input.Length)
                {
                    // Valid ws-escape at EOF
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
                    case 's': // KDL allows \s for space
                        sb.Append(' ');
                        i++;
                        break;
                    case ' ': // Handle 'a\\\ b' -> 'a\b' (Backslash followed by literal space is consumed)
                        i++;
                        break;
                    case 'u':
                        if (i + 2 < input.Length && input[i + 2] == '{')
                        {
                            // Find closing brace
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
                                    // Fallback if hex invalid
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
                        // Treat unknown escapes as literal backslash
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
