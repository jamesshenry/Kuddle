using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Kuddle.Serialization;

/// <summary>
/// Represents a mapping from a C# property to a KDL entry (argument, property, or child node).
/// </summary>
internal sealed record PropertyMapping(
    PropertyInfo Property,
    KdlArgumentAttribute? Argument,
    KdlPropertyAttribute? KdlProperty,
    KdlNodeAttribute? ChildNode
)
{
    public string GetPropertyKey() => KdlProperty?.Key ?? Property.Name.ToLowerInvariant();

    public string GetChildNodeName() => ChildNode?.Name ?? Property.Name.ToLowerInvariant();
}

/// <summary>
/// Cached metadata about how a CLR type maps to/from KDL nodes.
/// </summary>
///
[DebuggerDisplay("Type = {Type.Name,nq}")]
internal sealed class TypeMetadata
{
    private static readonly ConcurrentDictionary<Type, TypeMetadata> s_cache = new();

    public Type Type { get; }
    public string NodeName { get; }
    public bool IsNodeDefinition => Arguments.Count > 0 || Properties.Count > 0;

    /// <summary>Properties mapped to KDL arguments, sorted by index.</summary>
    public IReadOnlyList<PropertyMapping> Arguments { get; }

    /// <summary>Properties mapped to KDL properties.</summary>
    public IReadOnlyList<PropertyMapping> Properties { get; }

    /// <summary>Properties mapped to child nodes.</summary>
    public IReadOnlyList<PropertyMapping> Children { get; }

    /// <summary>All writable, non-ignored properties.</summary>
    public IReadOnlyList<PropertyMapping> AllMappings { get; }

    private TypeMetadata(Type type)
    {
        Type = type;

        // Determine node name from [KdlType] or fall back to type name
        var kdlTypeAttr = type.GetCustomAttribute<KdlTypeAttribute>();
        NodeName = kdlTypeAttr?.Name ?? type.Name.ToLowerInvariant();

        // Gather all writable, non-ignored properties
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite && p.GetCustomAttribute<KdlIgnoreAttribute>() == null)
            .Select(p => new PropertyMapping(
                p,
                p.GetCustomAttribute<KdlArgumentAttribute>(),
                p.GetCustomAttribute<KdlPropertyAttribute>(),
                p.GetCustomAttribute<KdlNodeAttribute>()
            ))
            .ToList();

        AllMappings = props;

        Arguments = props
            .Where(m => m.Argument is not null)
            .OrderBy(m => m.Argument!.Index)
            .ToList();

        Properties = props.Where(m => m.KdlProperty is not null).ToList();

        Children = props.Where(m => m.ChildNode is not null).ToList();
    }

    /// <summary>
    /// Gets or creates cached metadata for a type.
    /// </summary>
    public static TypeMetadata For(Type type) => s_cache.GetOrAdd(type, t => new TypeMetadata(t));

    /// <summary>
    /// Gets or creates cached metadata for a type.
    /// </summary>
    public static TypeMetadata For<T>() => For(typeof(T));

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
        Type.IsGenericType
        && Type.GetInterfaces()
            .Any(i =>
                i.IsGenericType
                && (
                    i.GetGenericTypeDefinition() == typeof(IDictionary<,>)
                    || i.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>)
                )
            );
}
