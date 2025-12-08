using System;

namespace Kuddle.Exceptions;

[Serializable]
internal class KdlParseException : Exception
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
}
