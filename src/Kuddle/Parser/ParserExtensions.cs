using Parlot.Fluent;

namespace Kuddle.Parser;

public static class ParserExtensions
{
    public static Parser<T> Debug<T>(this Parser<T> parser, string name)
    {
        return new DebugParser<T>(parser, name);
    }
}
