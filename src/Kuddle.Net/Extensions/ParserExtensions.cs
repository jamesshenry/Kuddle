using System.Diagnostics;
using Kuddle.Parser;
using Parlot.Fluent;

namespace Kuddle.Extensions;

internal static class ParserExtensions
{
    /// <summary>
    /// Wraps a parser with debug tracing. Only active in DEBUG builds.
    /// In Release builds, this is a no-op to enable Parlot compilation.
    /// </summary>
    [DebuggerStepThrough]
    public static Parser<T> Debug<T>(this Parser<T> parser, string name, bool enableDebug = false)
    {
#if DEBUG
        if (enableDebug)
            return new DebugParser<T>(parser, name);
        else
            return parser;
#else
        return parser;
#endif
    }
}
