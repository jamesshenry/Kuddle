using System;

namespace Kuddle.Exceptions;

[Serializable]
public class KuddleParseException : Exception
{
    private readonly Exception? _ex = default;

    public KuddleParseException() { }

    public KuddleParseException(Exception ex)
    {
        _ex = ex;
    }

    public KuddleParseException(string? message)
        : base(message) { }

    public KuddleParseException(string? message, Exception? innerException)
        : base(message, innerException) { }

    public KuddleParseException(string? message, int? column, int? line, int? offset)
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
