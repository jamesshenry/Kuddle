using System;

namespace Kuddle.AST;

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
