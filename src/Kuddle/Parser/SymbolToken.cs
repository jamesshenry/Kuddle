using Parlot.Fluent;
using static Parlot.Fluent.Parsers;

namespace Kuddle.Parser;

public static class SymbolToken
{
    public static readonly Parser<char> OpenParen = Literals.Char('(');
    public static readonly Parser<char> CloseParen = Terms.Char(')');
    public static readonly Parser<char> OpenBrace = Terms.Char('{');
    public static readonly Parser<char> CloseBrace = Terms.Char('}');
    public static readonly Parser<char> SingleQuote = Terms.Char('"');
    public static readonly Parser<string> DoubleQuote = Terms.Text("\"\"");
    public static readonly Parser<string> SlashStar = Terms.Text("/*");
    public static readonly Parser<string> StarSlash = Terms.Text("*/");
    public static readonly Parser<char> Star = Terms.Char('*');
    public static readonly Parser<char> Slash = Terms.Char('/');
}
