using System;
using System.Globalization;
using System.Linq;

namespace Kuddle.AST;

public abstract record KdlValue : KdlObject
{
    public string? TypeAnnotation { get; init; }

    /// <summary>Gets a KdlNull value.</summary>
    public static KdlValue Null => new KdlNull();

    /// <summary>Creates a KdlString from a Guid with "uuid" type annotation.</summary>
    public static KdlString From(Guid guid, StringKind stringKind = StringKind.Quoted)
    {
        return new KdlString(guid.ToString(), stringKind) { TypeAnnotation = "uuid" };
    }

    /// <summary>Creates a KdlString from a DateTime with "date-time" type annotation.</summary>
    public static KdlString From(DateTime date, StringKind stringKind = StringKind.Quoted)
    {
        // Using "O" ensures it includes the Z or Offset for KDL compliance
        return new KdlString(date.ToString("O"), stringKind) { TypeAnnotation = "date-time" };
    }

    /// <summary>Creates a KdlString from a DateTimeOffset with "date-time" type annotation (ISO 8601).</summary>
    public static KdlString From(DateTimeOffset date, StringKind stringKind = StringKind.Quoted)
    {
        return new KdlString(date.ToString("O"), stringKind) { TypeAnnotation = "date-time" };
    }

    public static KdlString From(DateOnly date, StringKind stringKind = StringKind.Quoted)
    {
        return new KdlString(date.ToString("O"), stringKind) { TypeAnnotation = "date" };
    }

    public static KdlString From(TimeOnly time, StringKind stringKind = StringKind.Quoted)
    {
        return new KdlString(time.ToString("O"), stringKind) { TypeAnnotation = "time" };
    }

    internal static KdlString From(TimeSpan timeSpan, StringKind stringKind = StringKind.Quoted)
    {
        return new KdlString(timeSpan.ToString("g"), stringKind) { TypeAnnotation = "duration" };
    }

    public static KdlString From(Enum e)
    {
        return new KdlString(e.ToString(), StringKind.Bare);
    }

    public static KdlString From(string value, StringKind stringKind = StringKind.Bare)
    {
        if (stringKind == StringKind.Bare && value.Any(char.IsWhiteSpace))
            stringKind = StringKind.Quoted;
        return new KdlString(value, stringKind);
    }

    public static KdlBool From(bool value) => new(value);

    public static KdlNumber From(int value) => new(value.ToString(CultureInfo.InvariantCulture));

    public static KdlNumber From(long value) => new(value.ToString(CultureInfo.InvariantCulture));

    public static KdlNumber From(double value) =>
        new(value.ToString("G17", CultureInfo.InvariantCulture));

    public static KdlNumber From(decimal value) =>
        new(value.ToString(CultureInfo.InvariantCulture));
}
