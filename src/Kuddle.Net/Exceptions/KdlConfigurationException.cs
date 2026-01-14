using System;

namespace Kuddle.Exceptions;

[Serializable]
internal class KdlConfigurationException : Exception
{
    public KdlConfigurationException() { }

    public KdlConfigurationException(string? message)
        : base(message) { }

    public KdlConfigurationException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
