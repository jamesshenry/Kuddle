using Kuddle.AST;
using Parlot;
using Parlot.Fluent;

namespace Kuddle.Parser;

public class RawStringParser : Parser<KdlString>
{
    public override bool Parse(ParseContext context, ref ParseResult<KdlString> result)
    {
        var cursor = context.Scanner.Cursor;
        var buffer = context.Scanner.Buffer;
        var beforeHashPos = cursor.Position;

        if (cursor.Current != '#')
            return false;

        int openHashCount = 0;
        while (cursor.Current == '#')
        {
            openHashCount++;
            cursor.Advance();
        }

        int openQuoteCount = 0;
        while (cursor.Current == '"')
        {
            openQuoteCount++;
            cursor.Advance();
        }

        if (openQuoteCount == 0)
        {
            cursor.ResetPosition(beforeHashPos);
            return false;
        }

        var start = cursor.Position;

        while (!cursor.Eof)
        {
            if (cursor.Current == '"')
            {
                var potentialEnd = cursor.Position;

                int closeQuoteCount = 0;
                while (closeQuoteCount < openQuoteCount && cursor.Current == '"')
                {
                    closeQuoteCount++;
                    cursor.Advance();
                }

                int closeHashCount = 0;
                if (closeQuoteCount == openQuoteCount)
                {
                    while (closeHashCount < openHashCount && cursor.Current == '#')
                    {
                        closeHashCount++;
                        cursor.Advance();
                    }
                }

                if (closeQuoteCount == openQuoteCount && closeHashCount == openHashCount)
                {
                    var length = potentialEnd.Offset - start.Offset;
                    var content = buffer[start.Offset..potentialEnd.Offset];

                    content = KuddleGrammar.Dedent(content);

                    result.Set(start.Offset, length, new KdlString(content, StringKind.Raw));
                    return true;
                }
            }
            else
            {
                cursor.Advance();
            }
        }
        throw new ParseException(
            $"Expected raw string to have {openHashCount} closing hashes",
            cursor.Position
        );
    }
}
