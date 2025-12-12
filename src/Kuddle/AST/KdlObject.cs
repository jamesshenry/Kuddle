using System;
using System.Collections.Generic;
using Kuddle.Serialization;

namespace Kuddle.AST;

public record KdlObject
{
    public string LeadingTrivia { get; init; } = string.Empty;
    public string TrailingTrivia { get; init; } = string.Empty;
}

public sealed record KdlDocument : KdlObject
{
    public List<KdlNode> Nodes { get; init; } = [];

    public string ToString(KdlWriterOptions? options = null)
    {
        return KdlWriter.Write(this, options);
    }

    public override string ToString()
    {
        return KdlWriter.Write(this);
    }
}

public sealed record KdlBlock : KdlObject
{
    public List<KdlNode> Nodes { get; init; } = [];
}

public sealed record KdlNode(KdlString Name) : KdlObject
{
    public List<KdlEntry> Entries { get; init; } = [];

    public KdlBlock? Children { get; init; }

    public bool TerminatedBySemicolon { get; init; }
    public string? TypeAnnotation { get; init; }
}

public abstract record KdlEntry : KdlObject;

public sealed record KdlArgument(KdlValue Value) : KdlEntry;

public sealed record KdlProperty(KdlString Key, KdlValue Value) : KdlEntry
{
    public string EqualsTrivia { get; init; } = "=";
}

public sealed record KdlSkippedEntry(string RawText) : KdlEntry;

// public sealed record KdlIdentifier(string Name) : KdlObject
// {
//     public string RawText { get; init; } = Name;

//     public string? TypeAnnotation { get; init; }
// }

public abstract record KdlValue : KdlObject
{
    public string? TypeAnnotation { get; init; }
    public static KdlValue Null => new KdlNull();

    internal static KdlString From(Guid guid, StringKind stringKind = StringKind.Quoted)
    {
        return new KdlString(guid.ToString(), stringKind) { TypeAnnotation = "uuid" };
    }

    internal static KdlString From(DateTimeOffset date, StringKind stringKind = StringKind.Quoted)
    {
        return new KdlString(date.ToString("O"), stringKind) { TypeAnnotation = "date-time" };
    }
}

[Flags]
public enum StringKind
{
    Bare = 1,
    Quoted = 2,
    Raw = 4,
    MultiLine = 8,

    MultiLineRaw = MultiLine | Raw,
    QuotedRaw = Quoted | Raw,
}

public sealed record KdlString(string Value, StringKind Kind) : KdlValue
{
    public override string ToString() => Value;
}

public sealed record KdlBool(bool Value) : KdlValue;

public sealed record KdlNull : KdlValue;
