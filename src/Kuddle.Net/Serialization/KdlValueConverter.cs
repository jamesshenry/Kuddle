using System;
using Kuddle.AST;
using Kuddle.Extensions;
using Kuddle.Parser;

namespace Kuddle.Serialization;

/// <summary>
/// Provides unified conversion between CLR values and KDL values.
/// </summary>
internal static class KdlValueConverter
{
    /// <summary>
    /// Attempts to convert a KDL value to a CLR type.
    /// </summary>
    public static bool TryFromKdl(KdlValue kdlValue, Type targetType, out object? value)
    {
        value = default;

        if (kdlValue is KdlNull)
        {
            if (!IsNullable(targetType))
            {
                return false;
            }
            value = null;
            return true;
        }

        var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (underlying.IsEnum)
        {
            kdlValue.TryGetString(out var enumString);
            bool success = Enum.TryParse(underlying, enumString, true, out var result);
            value = result;
            return success;
        }
        // String
        if (underlying == typeof(string) && kdlValue.TryGetString(out var stringVal))
        {
            value = stringVal;
            return true;
        }

        // Integers (signed)
        if (underlying == typeof(int) && kdlValue.TryGetInt(out var intVal))
        {
            value = intVal;
            return true;
        }

        if (underlying == typeof(long) && kdlValue.TryGetLong(out var longVal))
        {
            value = longVal;
            return true;
        }

        if (underlying == typeof(short) && kdlValue.TryGetInt(out var shortVal))
        {
            value = (short)shortVal;
            return true;
        }

        if (underlying == typeof(sbyte) && kdlValue.TryGetInt(out var sbyteVal))
        {
            value = (sbyte)sbyteVal;
            return true;
        }

        // Integers (unsigned)
        if (underlying == typeof(uint) && kdlValue.TryGetLong(out var uintVal))
        {
            value = (uint)uintVal;
            return true;
        }

        if (underlying == typeof(ulong) && kdlValue.TryGetLong(out var ulongVal))
        {
            value = (ulong)ulongVal;
            return true;
        }

        if (underlying == typeof(ushort) && kdlValue.TryGetInt(out var ushortVal))
        {
            value = (ushort)ushortVal;
            return true;
        }

        if (underlying == typeof(byte) && kdlValue.TryGetInt(out var byteVal))
        {
            value = (byte)byteVal;
            return true;
        }

        // Floating point
        if (underlying == typeof(double) && kdlValue.TryGetDouble(out var doubleVal))
        {
            value = doubleVal;
            return true;
        }

        if (underlying == typeof(decimal) && kdlValue.TryGetDecimal(out var decimalVal))
        {
            value = decimalVal;
            return true;
        }

        if (underlying == typeof(float) && kdlValue.TryGetDouble(out var floatVal))
        {
            value = (float)floatVal;
            return true;
        }

        // Boolean
        if (underlying == typeof(bool) && kdlValue.TryGetBool(out var boolVal))
        {
            value = boolVal;
            return true;
        }

        // Special types with type annotations
        if (underlying == typeof(Guid) && kdlValue.TryGetUuid(out var uuid))
        {
            value = uuid;
            return true;
        }

        if (underlying == typeof(DateTimeOffset) && kdlValue.TryGetDateTime(out var dto))
        {
            value = dto;
            return true;
        }

        if (underlying == typeof(DateTime) && kdlValue.TryGetDateTime(out var dt))
        {
            value = dt.DateTime;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Attempts to convert a CLR value to a KDL value.
    /// </summary>
    public static bool TryToKdl(object? input, out KdlValue kdlValue, string? typeAnnotation = null)
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
            short sh => KdlValue.From(sh),
            sbyte sb => KdlValue.From(sb),
            uint ui => KdlValue.From(ui),
            ulong ul => KdlValue.From((long)ul),
            ushort us => KdlValue.From(us),
            byte by => KdlValue.From(by),
            double d => KdlValue.From(d),
            float f => KdlValue.From((double)f),
            decimal m => KdlValue.From(m),
            bool b => KdlValue.From(b),
            Guid uuid => KdlValue.From(uuid),
            DateTimeOffset dto => KdlValue.From(dto),
            DateTime dt => KdlValue.From(new DateTimeOffset(dt)),
            _ => null!,
        };

        if (kdlValue is not null && typeAnnotation is not null)
        {
            kdlValue = kdlValue with { TypeAnnotation = typeAnnotation };
        }

        return kdlValue is not null;
    }

    /// <summary>
    /// Converts a KDL value to a CLR type, throwing on failure.
    /// </summary>
    public static object FromKdlOrThrow(
        KdlValue kdlValue,
        Type targetType,
        string context,
        string? expectedTypeAnnotation = null
    )
    {
        var finalTargetType =
            CharacterSets.GetClrType(expectedTypeAnnotation ?? kdlValue.TypeAnnotation)
            ?? targetType;

        if (!TryFromKdl(kdlValue, finalTargetType, out var result))
        {
            throw new KuddleSerializationException(
                $"Cannot convert KDL value '{kdlValue}' to {targetType.Name}. {context}"
            );
        }
        return result ?? throw new Exception();
    }

    /// <summary>
    /// Converts a CLR value to a KDL value, throwing on failure.
    /// </summary>
    public static KdlValue ToKdlOrThrow(
        object? input,
        string context,
        string? typeAnnotation = null
    )
    {
        if (!TryToKdl(input, out var kdlValue, typeAnnotation))
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
