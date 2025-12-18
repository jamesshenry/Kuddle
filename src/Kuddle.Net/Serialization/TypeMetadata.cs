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
internal sealed record KdlEntryMapping(PropertyInfo Property, KdlEntryAttribute? Entry)
{
    public bool IsArgument => Entry is KdlArgumentAttribute;
    public bool IsProperty => Entry is KdlPropertyAttribute;
    public bool IsChildNode => Entry is KdlNodeAttribute;

    public int ArgumentIndex => Entry is KdlArgumentAttribute arg ? arg.Index : -1;

    public string GetPropertyKey() =>
        Entry is KdlPropertyAttribute prop
            ? prop.Key ?? Property.Name.ToLowerInvariant()
            : Property.Name.ToLowerInvariant();

    public string GetChildNodeName() =>
        Entry is KdlNodeAttribute node
            ? node.Name ?? Property.Name.ToLowerInvariant()
            : Property.Name.ToLowerInvariant();

    public string? TypeAnnotation => Entry?.TypeAnnotation;
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
    public bool IsNodeDefinition => ArgumentAttributes.Count > 0 || Properties.Count > 0;

    /// <summary>Properties mapped to KDL arguments, sorted by index.</summary>
    public IReadOnlyList<KdlEntryMapping> ArgumentAttributes { get; }

    /// <summary>Properties mapped to KDL properties.</summary>
    public IReadOnlyList<KdlEntryMapping> Properties { get; }

    /// <summary>Properties mapped to child nodes.</summary>
    public IReadOnlyList<KdlEntryMapping> Children { get; }

    /// <summary>All writable, non-ignored properties.</summary>
    public IReadOnlyList<KdlEntryMapping> AllMappings { get; }

    private TypeMetadata(Type type)
    {
        Type = type;

        var kdlTypeAttr = type.GetCustomAttribute<KdlTypeAttribute>();
        NodeName = kdlTypeAttr?.Name ?? type.Name.ToLowerInvariant();

        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite && p.GetCustomAttribute<KdlIgnoreAttribute>() == null)
            .Select(p => new KdlEntryMapping(p, p.GetCustomAttribute<KdlEntryAttribute>()))
            .ToList();

        AllMappings = props;

        ArgumentAttributes = [.. props.Where(m => m.IsArgument).OrderBy(m => m.ArgumentIndex)];
        Properties = [.. props.Where(m => m.IsProperty)];
        Children = [.. props.Where(m => m.IsChildNode)];
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
