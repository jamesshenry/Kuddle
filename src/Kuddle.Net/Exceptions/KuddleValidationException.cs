using System;
using System.Collections.Generic;
using Kuddle.AST;

namespace Kuddle.Exceptions;

public class KuddleValidationException : Exception
{
    public IEnumerable<KuddleValidationError> Errors { get; } = [];

    public KuddleValidationException(List<KuddleValidationError> errors)
        : base($"Found {errors.Count} validation errors in the KDL document.")
    {
        Errors = errors;
    }

    public KuddleValidationException() { }

    public KuddleValidationException(string? message)
        : base(message) { }

    public KuddleValidationException(string? message, Exception? innerException)
        : base(message, innerException) { }
}

public record KuddleValidationError(string Message, KdlObject Source);
