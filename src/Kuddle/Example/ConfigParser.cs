using System.Numerics;
using System.Text.Json.Serialization;
using Parlot.Fluent;
using static Parlot.Fluent.Parsers;

namespace Kuddle.Example;

public abstract record JsonNode;

public record JsonArray : JsonNode;

public record JsonObject : JsonNode;

public abstract record JsonValue : JsonNode;

public abstract record JsonValue<T>(T Value) : JsonValue;

public record JsonBoolean(bool Value) : JsonValue<bool>(Value);

public record JsonString(string Value) : JsonValue<string>(Value);

public record JsonNumber(int Value) : JsonValue<int>(Value);
