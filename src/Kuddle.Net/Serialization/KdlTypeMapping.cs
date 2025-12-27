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
        NodeName = typeAttr?.Name ?? type.Name.ToKebabCase();

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

    private void ValidateMapping()
    {
        // Sort arguments and check for continuity
        var sortedArgs = Arguments.OrderBy(a => a.ArgumentIndex).ToList();
        for (int i = 0; i < sortedArgs.Count; i++)
        {
            if (sortedArgs[i].ArgumentIndex != i)
                throw new KdlConfigurationException(
                    $"Type '{Type.Name}' has non-contiguous KDL arguments. Expected index {i}, found {sortedArgs[i].ArgumentIndex}."
                );
        }

        Arguments.Clear();
        Arguments.AddRange(sortedArgs);

        foreach (var prop in Properties)
        {
            if (prop.IsDictionary && !prop.DictionaryValueProperty!.PropertyType.IsKdlScalar)
            {
                throw new KdlConfigurationException(
                    $"Property '{prop.Property.PropertyType.Name}.{prop.Property.Name}' is marked with [KdlProperty], "
                        + $"but its value type '{prop.DictionaryValueProperty!.Name}' is complex. "
                        + "KDL properties only support scalar values. Use [KdlNode] instead."
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
        internal bool IsKdlSerializable() =>
            info.CanWrite
            && info.GetCustomAttribute<KdlIgnoreAttribute>() == null
            && info.GetIndexParameters().Length == 0;
    }
}
