using System;

namespace Kuddle.Exceptions;

[Serializable]
public class KdlParseException : Exception
{
    private readonly Exception? _ex = default;

    public KdlParseException() { }

    public KdlParseException(Exception ex)
    {
        _ex = ex;
    }

    public KdlParseException(string? message)
        : base(message) { }

    public KdlParseException(string? message, Exception? innerException)
        : base(message, innerException) { }

    public KdlParseException(string? message, int? column, int? line, int? offset)
        : this(message)
    {
        Column = column;
        Line = line;
        Offset = offset;
    }

    public int? Line { get; }
    public int? Column { get; }
    public int? Offset { get; }
}
