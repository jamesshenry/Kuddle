using System;
using Kuddle.AST;
using Kuddle.Extensions;

namespace Kuddle.Serialization;

/// <summary>
/// Provides unified conversion between CLR values and KDL values.
/// </summary>
internal static class KdlValueConverter
{
    /// <summary>
    /// Attempts to convert a KDL value to a CLR type.
    /// </summary>
    public static bool TryFromKdl(KdlValue kdlValue, Type targetType, out object? result)
    {
        result = null;

        if (kdlValue is KdlNull)
        {
            if (!IsNullable(targetType))
            {
                return false;
            }
            result = null;
            return true;
        }

        var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

        // String
        if (underlying == typeof(string) && kdlValue.TryGetString(out var stringVal))
        {
            result = stringVal;
            return true;
        }

        // Integers
        if (underlying == typeof(int) && kdlValue.TryGetInt(out var intVal))
        {
            result = intVal;
            return true;
        }

        if (underlying == typeof(long) && kdlValue.TryGetLong(out var longVal))
        {
            result = longVal;
            return true;
        }

        // Floating point
        if (underlying == typeof(double) && kdlValue.TryGetDouble(out var doubleVal))
        {
            result = doubleVal;
            return true;
        }

        if (underlying == typeof(decimal) && kdlValue.TryGetDecimal(out var decimalVal))
        {
            result = decimalVal;
            return true;
        }

        if (underlying == typeof(float) && kdlValue.TryGetDouble(out var floatVal))
        {
            result = (float)floatVal;
            return true;
        }

        // Boolean
        if (underlying == typeof(bool) && kdlValue.TryGetBool(out var boolVal))
        {
            result = boolVal;
            return true;
        }

        // Special types with type annotations
        if (underlying == typeof(Guid) && kdlValue.TryGetUuid(out var uuid))
        {
            result = uuid;
            return true;
        }

        if (underlying == typeof(DateTimeOffset) && kdlValue.TryGetDateTime(out var dto))
        {
            result = dto;
            return true;
        }

        if (underlying == typeof(DateTime) && kdlValue.TryGetDateTime(out var dt))
        {
            result = dt.DateTime;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Attempts to convert a CLR value to a KDL value.
    /// </summary>
    public static bool TryToKdl(object? input, out KdlValue kdlValue)
    {
        if (input is null)
        {
            kdlValue = KdlValue.Null;
            return true;
        }

        kdlValue = input switch
        {
            string s => KdlValue.From(s),
            int i => KdlValue.From(i),
            long l => KdlValue.From(l),
            double d => KdlValue.From(d),
            float f => KdlValue.From((double)f),
            decimal m => KdlValue.From(m),
            bool b => KdlValue.From(b),
            Guid uuid => KdlValue.From(uuid),
            DateTimeOffset dto => KdlValue.From(dto),
            DateTime dt => KdlValue.From(new DateTimeOffset(dt)),
            _ => null!,
        };

        return kdlValue is not null;
    }

    /// <summary>
    /// Converts a KDL value to a CLR type, throwing on failure.
    /// </summary>
    public static object? FromKdlOrThrow(KdlValue kdlValue, Type targetType, string context)
    {
        if (!TryFromKdl(kdlValue, targetType, out var result))
        {
            throw new KuddleSerializationException(
                $"Cannot convert KDL value '{kdlValue}' to {targetType.Name}. {context}"
            );
        }
        return result;
    }

    /// <summary>
    /// Converts a CLR value to a KDL value, throwing on failure.
    /// </summary>
    public static KdlValue ToKdlOrThrow(object? input, string context)
    {
        if (!TryToKdl(input, out var kdlValue))
        {
            var typeName = input?.GetType().Name ?? "null";
            throw new KuddleSerializationException(
                $"Cannot convert CLR value of type '{typeName}' to KDL. {context}"
            );
        }
        return kdlValue;
    }

    private static bool IsNullable(Type type) =>
        !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
}
