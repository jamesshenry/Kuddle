using System;

namespace Kuddle.Serialization;

[Serializable]
public class KuddleSerializationException : Exception
{
    private readonly Exception? _ex;

    public KuddleSerializationException() { }

    public KuddleSerializationException(Exception ex)
    {
        _ex = ex;
    }

    public KuddleSerializationException(string? message)
        : base(message) { }

    public KuddleSerializationException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
