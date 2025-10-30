using System.Numerics;
using System.Text.Json.Serialization;
using Parlot;
using Parlot.Fluent;
using static Parlot.Fluent.Parsers;

namespace Kuddle.Example;

public abstract record JsonNode;

public abstract record JsonValue : JsonNode;

public abstract record JsonValue<T>(T Value) : JsonValue;

public record JsonArray : JsonNode;

public record JsonObject : JsonNode;

public record JsonBoolean(bool Value) : JsonValue<bool>(Value);

public record JsonString(string Value) : JsonValue<string>(Value);

public record JsonNumber(int Value) : JsonValue<int>(Value);

public record JsonNull : JsonValue;

public static class JsonParser
{
    private static readonly Deferred<JsonNode> value;
    private static readonly Parser<JsonString> jsonString;
    private static readonly Parser<JsonNode> jsonArray;

    static JsonParser()
    {
        value = Deferred<JsonNode>();
        jsonString = Terms
            .String(StringLiteralQuotes.SingleOrDouble)
            .Then(s => new JsonString(new string(s.Span)));
        jsonArray = Between(Terms.Char('['), value, Terms.Char(']'));
    }
}
