using Parlot.Fluent;
using static Parlot.Fluent.Parsers;

namespace Kuddle;

public class KdlParser
{
    private static readonly KdlParser v2 = new();

    public KdlDocument Parse(string text)
    {
        return new KdlDocument();
    }

    public static KdlParser V2()
    {
        return v2;
    }
}

public class V2Parser
{
    public V2Parser()
    {
        var document = Deferred<KdlDocument>();
        Parser = document.Compile();
    }

    private readonly Parser<KdlDocument> Parser;

    public KdlDocument Parse(string text)
    {
        var result = Parser.TryParse(text, out var document);

        return document ?? throw new Exception();
    }
}

public record KdlDocument;
