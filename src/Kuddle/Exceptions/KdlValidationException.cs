using System;
using System.Collections.Generic;
using Kuddle.AST;

namespace Kuddle.Exceptions;

public class KdlValidationException : Exception
{
    public IEnumerable<KdlValidationError> Errors { get; } = [];

    public KdlValidationException(List<KdlValidationError> errors)
        : base($"Found {errors.Count} validation errors in the KDL document.")
    {
        Errors = errors;
    }

    public KdlValidationException() { }

    public KdlValidationException(string? message)
        : base(message) { }

    public KdlValidationException(string? message, Exception? innerException)
        : base(message, innerException) { }
}

public record KdlValidationError(string Message, KdlObject Source);
