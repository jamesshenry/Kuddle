using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Kuddle.Exceptions;
using Kuddle.Extensions;

namespace Kuddle.Serialization;

/// <summary>
/// Cached metadata about how a CLR type maps to/from KDL nodes.
/// </summary>
///
[DebuggerDisplay("Type = {Type.Name,nq}")]
internal sealed class KdlTypeInfo
{
    private static readonly ConcurrentDictionary<Type, KdlTypeInfo> s_cache = new();

    public Type Type { get; }
    public string NodeName { get; }

    public DictionaryInfo? DictionaryDef { get; }
    public Type? CollectionElementType { get; }

    /// <summary>
    /// A strict node cannot be a document node.
    /// Document nodes cannot have args or props
    /// </summary>
    public bool IsStrictNode =>
        ArgumentAttributes.Count > 0
        || Properties.Count > 0
        || Type.GetCustomAttribute<KdlTypeAttribute>() != null;

    /// <summary>Properties mapped to KDL arguments, sorted by index.</summary>
    public IReadOnlyList<KdlMemberInfo> ArgumentAttributes { get; }

    /// <summary>Properties mapped to KDL properties.</summary>
    public IReadOnlyList<KdlMemberInfo> Properties { get; }

    /// <summary>Properties mapped to child nodes.</summary>
    public IReadOnlyList<KdlMemberInfo> Children { get; }
    public IReadOnlyList<KdlMemberInfo> Dictionaries { get; }

    private KdlTypeInfo(Type type)
    {
        Type = type;

        var kdlTypeAttr = type.GetCustomAttribute<KdlTypeAttribute>();
        NodeName = kdlTypeAttr?.Name ?? type.Name.ToLowerInvariant();

        DictionaryDef = type.GetDictionaryInfo();
        CollectionElementType = type.GetCollectionInfo();
        var allMappings = new List<KdlMemberInfo>();

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (
                !prop.CanWrite
                || prop.GetCustomAttribute<KdlIgnoreAttribute>() != null
                || prop.GetIndexParameters().Length > 0 // Ignore indexers
            )
                continue;

            var attrs = prop.GetCustomAttributes()
                .Where(a => a is KdlEntryAttribute || a is KdlNodeDictionaryAttribute)
                .ToList();

            if (attrs.Count > 1)
                throw new KdlConfigurationException(
                    $"Property '{type.Name}.{prop.Name}' has multiple KDL attributes. Only one mapping is allowed per property."
                );

            if (attrs.Count == 0 && IsSystemCollectionProperty(prop))
                continue;
            Attribute mappingAttr = attrs.Count == 1 ? attrs[0] : InferAttribute(prop);
            allMappings.Add(new KdlMemberInfo(prop, mappingAttr));
        }

        var args = allMappings.Where(m => m.IsArgument).OrderBy(m => m.ArgumentIndex).ToList();
        for (int i = 0; i < args.Count; i++)
        {
            if (args[i].ArgumentIndex != i)
                throw new KdlConfigurationException(
                    $"Property '{type.Name}.{args[i].Property.Name}' declares index {args[i].ArgumentIndex}, but index {i} is missing. Arguments must be contiguous starting at 0."
                );
        }

        ArgumentAttributes = args;
        Properties = allMappings.Where(m => m.IsProperty).ToList();
        Children = allMappings.Where(m => m.IsNode).ToList();

        Dictionaries = allMappings
            .Where(m =>
                m.IsNodeDictionary /* TODO: Add support for IsPropertyDictionary and IsKeyedNodeCollection */
            )
            .ToList();
    }

    private static bool IsSystemCollectionProperty(PropertyInfo prop) =>
        prop.DeclaringType != null
        && prop.DeclaringType.Namespace != null
        && prop.DeclaringType.Namespace.StartsWith("system");

    /// <summary>
    /// Gets or creates cached metadata for a type.
    /// </summary>
    public static KdlTypeInfo For(Type type) => s_cache.GetOrAdd(type, t => new KdlTypeInfo(t));

    /// <summary>
    /// Gets or creates cached metadata for a type.
    /// </summary>
    public static KdlTypeInfo For<T>() => For(typeof(T));

    /// <summary>
    /// Checks if a type is a complex type (not primitive, not string, not interface/abstract).
    /// </summary>
    public bool IsComplexType =>
        !Type.IsValueType
        && !Type.IsPrimitive
        && Type != typeof(string)
        && Type != typeof(object)
        && !Type.IsInterface
        && !Type.IsAbstract;

    /// <summary>
    /// Checks if a type is enumerable (but not string or dictionary).
    /// </summary>
    public bool IsIEnumerable =>
        Type != typeof(string) && !IsDictionary && typeof(IEnumerable).IsAssignableFrom(Type);

    /// <summary>
    /// Checks if a type is a dictionary.
    /// </summary>
    public bool IsDictionary =>
        Type.GetInterfaces()
            .Any(i =>
                i.IsGenericType
                && (
                    i.GetGenericTypeDefinition() == typeof(IDictionary<,>)
                    || i.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>)
                )
            );

    private static Attribute InferAttribute(PropertyInfo prop) =>
        prop.PropertyType switch
        {
            { IsDictionary: true } => new KdlNodeDictionaryAttribute(prop.Name.ToKebabCase()),
            { IsIEnumerable: true } => new KdlNodeAttribute(prop.Name.ToKebabCase()),
            { IsKdlScalar: true } => new KdlPropertyAttribute(prop.Name.ToKebabCase()),
            _ => new KdlNodeAttribute(prop.Name.ToKebabCase()),
        };
}

internal sealed record DictionaryInfo(Type KeyType, Type ValueType);
