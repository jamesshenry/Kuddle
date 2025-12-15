using System;
using System.Globalization;

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

    /// <summary>Creates a KdlString from a DateTimeOffset with "date-time" type annotation (ISO 8601).</summary>
    public static KdlString From(DateTimeOffset date, StringKind stringKind = StringKind.Quoted)
    {
        return new KdlString(date.ToString("O"), stringKind) { TypeAnnotation = "date-time" };
    }

    /// <summary>Creates a KdlString from a string value.</summary>
    public static KdlString From(string value, StringKind stringKind = StringKind.Quoted)
    {
        return new KdlString(value, stringKind);
    }

    /// <summary>Creates a KdlBool from a boolean value.</summary>
    public static KdlBool From(bool value)
    {
        return new KdlBool(value);
    }

    /// <summary>Creates a KdlNumber from an integer value.</summary>
    public static KdlNumber From(int value)
    {
        return new KdlNumber(value.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>Creates a KdlNumber from a long value.</summary>
    public static KdlNumber From(long value)
    {
        return new KdlNumber(value.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>Creates a KdlNumber from a double value.</summary>
    public static KdlNumber From(double value)
    {
        return new KdlNumber(value.ToString("G17", CultureInfo.InvariantCulture));
    }

    /// <summary>Creates a KdlNumber from a decimal value.</summary>
    public static KdlNumber From(decimal value)
    {
        return new KdlNumber(value.ToString(CultureInfo.InvariantCulture));
    }
}
