using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Kuddle.AST;
using Kuddle.Exceptions;

namespace Kuddle.Validation;

public static class KdlReservedTypeValidator
{
    private static readonly HashSet<string> ReservedTypes =
    [
        "i8",
        "i16",
        "i32",
        "i64",
        "u8",
        "u16",
        "u32",
        "u64",
        "f32",
        "f64",
        "decimal64",
        "decimal128",
        "date-time",
        "time",
        "date",
        "duration",
        "decimal",
        "currency",
        "country-2",
        "country-3",
        "ipv4",
        "ipv6",
        "url",
        "uuid",
        "regex",
        "base64",
    ];

    public static void Validate(KdlDocument doc)
    {
        var errors = new List<KuddleValidationError>();

        foreach (var node in doc.Nodes)
        {
            ValidateNode(node, errors);
        }

        if (errors.Count > 0)
        {
            throw new KuddleValidationException(errors);
        }
    }

    private static void ValidateNode(KdlNode node, List<KuddleValidationError> errors)
    {
        foreach (var entry in node.Entries)
        {
            if (entry is KdlArgument arg)
            {
                ValidateValue(arg.Value, errors);
            }
            else if (entry is KdlProperty prop)
            {
                ValidateValue(prop.Value, errors);
            }
        }

        if (node.Children != null)
        {
            foreach (var child in node.Children.Nodes)
            {
                ValidateNode(child, errors);
            }
        }
    }

    private static void ValidateValue(KdlValue val, List<KuddleValidationError> errors)
    {
        if (val.TypeAnnotation == null)
            return;
        if (!ReservedTypes.Contains(val.TypeAnnotation))
            return;

        try
        {
            switch (val.TypeAnnotation)
            {
                // --- Integers ---
                case "u8":
                    EnsureNumber(val).ToByte();
                    break;
                case "u16":
                    EnsureNumber(val).ToUInt16();
                    break;
                case "u32":
                    EnsureNumber(val).ToUInt32();
                    break;
                case "u64":
                    EnsureNumber(val).ToUInt64();
                    break;
                case "i8":
                    EnsureNumber(val).ToSByte();
                    break;
                case "i16":
                    EnsureNumber(val).ToInt16();
                    break;
                case "i32":
                    EnsureNumber(val).ToInt32();
                    break;
                case "i64":
                    EnsureNumber(val).ToInt64();
                    break;

                // --- Floats ---
                case "f32":
                    // ToFloat() handles the parsing. We just check if it throws.
                    EnsureNumber(val).ToFloat();
                    break;
                case "f64":
                    EnsureNumber(val).ToDouble();
                    break;

                // --- Strings ---
                case "uuid":
                    if (!Guid.TryParse(EnsureString(val), out _))
                        throw new FormatException();
                    break;
                case "date-time":
                    if (!DateTimeOffset.TryParse(EnsureString(val), out _))
                        throw new FormatException();
                    break;
                case "ipv4":
                    if (
                        !IPAddress.TryParse(EnsureString(val), out var ip4)
                        || ip4.AddressFamily != AddressFamily.InterNetwork
                    )
                        throw new FormatException();
                    break;
                case "ipv6":
                    if (
                        !IPAddress.TryParse(EnsureString(val), out var ip6)
                        || ip6.AddressFamily != AddressFamily.InterNetworkV6
                    )
                        throw new FormatException();
                    break;
                case "url":
                    if (!Uri.TryCreate(EnsureString(val), UriKind.Absolute, out _))
                        throw new FormatException();
                    break;
                case "base64":
                    Convert.FromBase64String(EnsureString(val));
                    break;
                case "regex":
                    try
                    {
                        _ = new Regex(EnsureString(val));
                    }
                    catch
                    {
                        throw new FormatException();
                    }
                    break;
            }
        }
        catch (Exception ex) when (ex.Message.StartsWith("Expected a"))
        {
            errors.Add(new KuddleValidationError(ex.Message, val));
        }
        catch (Exception)
        {
            errors.Add(
                new KuddleValidationError(
                    $"Value '{val}' is not a valid '{val.TypeAnnotation}'.",
                    val
                )
            );
        }
    }

    private static KdlNumber EnsureNumber(KdlValue val) =>
        val is KdlNumber num
            ? num
            : throw new FormatException(
                $"Expected a Number for type '{val.TypeAnnotation}', got {val.GetType().Name}"
            );

    private static string EnsureString(KdlValue val) =>
        val is KdlString str
            ? str.Value
            : throw new FormatException(
                $"Expected a String for type '{val.TypeAnnotation}', got {val.GetType().Name}"
            );
}
