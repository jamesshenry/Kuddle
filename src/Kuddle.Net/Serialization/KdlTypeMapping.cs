using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Kuddle.AST;
using Kuddle.Exceptions;
using Kuddle.Extensions;

namespace Kuddle.Serialization;

internal sealed record KdlTypeMapping
{
    private static readonly ConcurrentDictionary<Type, KdlTypeMapping> s_cache = new();

    private KdlTypeMapping(Type type)
    {
        Type = type;

        var typeAttr = type.GetCustomAttribute<KdlTypeAttribute>();
        NodeName = typeAttr?.Name ?? (type.IsAnonymousType() ? "-" : type.Name.ToKebabCase());
        IsDictionary = type.IsDictionary;
        if (IsDictionary)
        {
            var elementType = type.GetCollectionElementType();
            DictionaryKeyProperty = elementType?.GetProperty("Key");
            DictionaryValueProperty = elementType?.GetProperty("Value");
        }
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in props)
        {
            if (!prop.IsKdlSerializable())
                continue;
            if (prop.GetCustomAttribute<KdlExtensionDataAttribute>() != null)
            {
                if (!prop.PropertyType.IsDictionary)
                    throw new KdlConfigurationException(
                        $"ExtensionData property '{prop.Name}' must be a Dictionary."
                    );

                ExtensionDataProperty = new KdlMemberMap(
                    prop,
                    KdlMemberKind.ExtensionData,
                    "##extension_data##"
                );
                continue;
            }
            var map = CreateMemberMap(prop);

            switch (map.Kind)
            {
                case KdlMemberKind.Argument:
                    Arguments.Add(map);
                    break;
                case KdlMemberKind.Property:
                    Properties.Add(map);
                    break;
                case KdlMemberKind.ChildNode:
                    Children.Add(map);
                    break;
            }
        }
        ValidateMapping();
    }

    public Type Type { get; init; }
    public string NodeName { get; init; }
    public bool IsDictionary { get; }
    public List<KdlMemberMap> Properties { get; } = [];
    public List<KdlMemberMap> Children { get; } = [];
    internal List<KdlMemberMap> Arguments { get; } = [];
    public PropertyInfo? DictionaryKeyProperty { get; }
    public PropertyInfo? DictionaryValueProperty { get; }
    public bool HasMembers => Arguments.Count > 0 || Properties.Count > 0 || Children.Count > 0;
    public KdlMemberMap? ExtensionDataProperty { get; private set; }

    private void ValidateMapping()
    {
        // --- Rule 5: Slot Uniqueness (Properties & Children) ---
        var usedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var prop in Properties)
        {
            if (!usedKeys.Add(prop.KdlName))
            {
                throw new KdlConfigurationException(
                    $"Type '{Type.Name}' has multiple properties mapping to the KDL key '{prop.KdlName}'."
                );
            }
        }

        // Children usually share the same name if they are part of a collection,
        // but distinct properties shouldn't map to the same node name unless flattened.
        var usedChildNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var child in Children)
        {
            if (!child.IsCollection && !usedChildNames.Add(child.KdlName))
            {
                throw new KdlConfigurationException(
                    $"Type '{Type.Name}' has multiple non-collection members mapping to the KDL node '{child.KdlName}'."
                );
            }
        }

        // --- Rule 9: Argument Ambiguity (Rest Position) ---
        // Sort arguments by index to check sequence
        var sortedArgs = Arguments.OrderBy(a => a.ArgumentIndex).ToList();

        for (int i = 0; i < sortedArgs.Count; i++)
        {
            // Check for continuity (Existing logic)
            if (sortedArgs[i].ArgumentIndex != i)
            {
                throw new KdlConfigurationException(
                    $"Type '{Type.Name}' has non-contiguous KDL arguments. Expected index {i}, found {sortedArgs[i].ArgumentIndex}."
                );
            }

            // Rule 9: If this is a collection (Rest argument), it MUST be the last one.
            if (sortedArgs[i].IsCollection && i < sortedArgs.Count - 1)
            {
                throw new KdlConfigurationException(
                    $"Property '{sortedArgs[i].Property.Name}' in type '{Type.Name}' is a collection argument (Rest argument) "
                        + $"at index {i}, but it is followed by another argument. Rest arguments must be the final argument."
                );
            }
        }

        // --- Rule 16: Illegal Flattening ---
        foreach (var child in Children)
        {
            if (child.IsFlattened)
            {
                // Flattening a scalar (int, string, bool) makes no sense in KDL
                // because there are no properties or sub-nodes to hoist.
                if (child.Property.PropertyType.IsKdlScalar)
                {
                    throw new KdlConfigurationException(
                        $"Property '{child.Property.Name}' in type '{Type.Name}' has Flatten=true, "
                            + $"but the type '{child.Property.PropertyType.Name}' is a scalar. Flattening is only supported for collections or complex types."
                    );
                }
            }
        }

        // Existing Dictionary validation
        foreach (var prop in Properties)
        {
            if (prop.IsDictionary && !prop.DictionaryValueProperty!.PropertyType.IsKdlScalar)
            {
                throw new KdlConfigurationException(
                    $"Property '{prop.Property.Name}' is marked as [KdlProperty] but contains a complex Dictionary. "
                        + "Use [KdlNode] for dictionaries of complex objects."
                );
            }
        }
    }

    private static KdlMemberMap CreateMemberMap(PropertyInfo prop)
    {
        var all = prop.GetCustomAttributes<KdlEntryAttribute>();
        if (prop.GetCustomAttribute<KdlEntryAttribute>() is not KdlEntryAttribute attr)
        {
            attr = InferAttribute(prop);
        }
        var typeAnnotation = attr.TypeAnnotation;

        return attr switch
        {
            KdlArgumentAttribute arg => new KdlMemberMap(
                prop,
                KdlMemberKind.Argument,
                "",
                arg.Index,
                typeAnnotation
            ),
            KdlPropertyAttribute p => new KdlMemberMap(
                prop,
                KdlMemberKind.Property,
                prop.PropertyType.IsDictionary
                    ? (p.Key ?? string.Empty)
                    : (p.Key ?? prop.Name.ToKebabCase()),
                -1,
                typeAnnotation
            ),
            KdlNodeAttribute n => new KdlMemberMap(
                prop,
                KdlMemberKind.ChildNode,
                n.Name ?? prop.Name.ToKebabCase(),
                -1,
                typeAnnotation,
                n.Flatten,
                collectionElementName: n.ElementName
            ),

            // KdlNodeDictionaryAttribute nd => new KdlMemberMap(
            //     prop,
            //     KdlMemberKind.ChildNode,
            //     nd.Name ?? prop.Name.ToKebabCase(),
            //     -1,
            //     typeAnnotation
            // ),
            _ => new KdlMemberMap(
                prop,
                KdlMemberKind.ChildNode,
                prop.Name.ToKebabCase(),
                -1,
                typeAnnotation
            ),
        };
    }

    private static KdlEntryAttribute InferAttribute(PropertyInfo prop) =>
        prop.PropertyType switch
        {
            { IsDictionary: true } => new KdlNodeAttribute(prop.Name.ToKebabCase()),
            { IsIEnumerable: true } => new KdlNodeAttribute(prop.Name.ToKebabCase()),
            { IsKdlScalar: true } => new KdlPropertyAttribute(prop.Name.ToKebabCase()),
            _ => new KdlNodeAttribute(prop.Name.ToKebabCase()),
        };

    /// <summary>
    /// Gets or creates cached metadata for a type.
    /// </summary>
    public static KdlTypeMapping For(Type type) =>
        s_cache.GetOrAdd(type, t => new KdlTypeMapping(t));

    /// <summary>
    /// Gets or creates cached metadata for a type.
    /// </summary>
    public static KdlTypeMapping For<T>() => For(typeof(T));
}

public static class KdlTypeMappingExtensions
{
    extension(KdlValue value) { }

    extension(PropertyInfo info)
    {
        internal bool IsKdlSerializable()
        {
            // 1. Basic checks
            if (
                info.GetMethod == null
                || info.GetCustomAttribute<KdlIgnoreAttribute>() != null
                || info.GetIndexParameters().Length > 0
            )
            {
                return false;
            }

            // 2. Filter out BCL Collection infrastructure (Comparer, Count, Keys, etc.)
            var declaringType = info.DeclaringType;
            if (declaringType != null && declaringType.IsGenericType)
            {
                var genDef = declaringType.GetGenericTypeDefinition();
                if (
                    genDef == typeof(Dictionary<,>)
                    || genDef == typeof(List<>)
                    || declaringType.Namespace?.StartsWith("System.Collections") == true
                )
                {
                    return false;
                }
            }

            return true;
        }
    }
}
