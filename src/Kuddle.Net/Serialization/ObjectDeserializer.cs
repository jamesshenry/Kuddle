using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kuddle.AST;
using Kuddle.Extensions;

namespace Kuddle.Serialization;

internal class ObjectDeserializer
{
    private const StringComparison NodeNameComparison = StringComparison.OrdinalIgnoreCase;

    private readonly KdlSerializerOptions _options;

    public ObjectDeserializer(KdlSerializerOptions? options = null)
    {
        _options = options ?? KdlSerializerOptions.Default;
    }

    internal static T DeserializeDocument<T>(KdlDocument doc, KdlSerializerOptions? options)
        where T : new()
    {
        var worker = new ObjectDeserializer(options);

        var mapping = KdlTypeMapping.For<T>();
        var instance = new T();

        if (mapping.Arguments.Count > 0 || mapping.Properties.Count > 0)
        {
            var matches = doc
                .Nodes.Where(n => n.Name.Value.Equals(mapping.NodeName, NodeNameComparison))
                .ToList();

            if (matches.Count == 0)
            {
                // If the document has content, but none of it is the node we want, it's an error.
                if (doc.Nodes.Count > 0)
                {
                    var foundNames = string.Join(", ", doc.Nodes.Select(n => $"'{n.Name.Value}'"));
                    throw new KuddleSerializationException(
                        $"Expected root node '{mapping.NodeName}', but found: {foundNames}."
                    );
                }
                return instance; // Document is totally empty; return empty object
            }

            // THROW: Ambiguity check
            if (matches.Count > 1)
            {
                throw new KuddleSerializationException(
                    $"Found {matches.Count} nodes matching '{mapping.NodeName}', but only 1 was expected. "
                        + "To deserialize a list of nodes, use KdlSerializer.DeserializeMany<T>()."
                );
            }

            worker.MapNodeToObject(matches[0], instance, mapping);
        }
        else
        {
            // Mode B: Document Mode or Intrinsic Dictionary at root
            if (mapping.IsDictionary)
            {
                worker.PopulateDictionary(
                    (IDictionary)instance,
                    doc.Nodes,
                    mapping.DictionaryKeyProperty!.PropertyType,
                    mapping.DictionaryValueProperty!.PropertyType
                );
            }
            else
            {
                worker.MapChildren(doc.Nodes, instance, mapping);
            }
        }

        return instance;
    }

    internal static T DeserializeNode<T>(KdlNode node, KdlSerializerOptions? options)
        where T : new()
    {
        var worker = new ObjectDeserializer(options);
        var metadata = KdlTypeMapping.For<T>();
        ValidateNodeName(node, metadata.NodeName);

        var instance = new T();
        worker.MapNodeToObject(node, instance, metadata);
        return instance;
    }

    /// <summary>
    /// Maps a KDL node's entries and children to an object instance.
    /// </summary>
    private void MapNodeToObject(KdlNode node, object instance, KdlTypeMapping mapping)
    {
        foreach (var map in mapping.Arguments)
        {
            var kdlValue = node.Arg(map.ArgumentIndex);
            if (kdlValue != null)
            {
                var val = KdlValueConverter.FromKdlOrThrow(
                    kdlValue,
                    map.Property.PropertyType,
                    map.KdlName,
                    map.TypeAnnotation
                );
                map.SetValue(instance, val);
            }
        }
        var consumedPropKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var map in mapping.Properties.Where(m => !m.IsDictionary))
        {
            var kdlValue = node.Prop(map.KdlName);
            if (kdlValue != null)
            {
                var val = KdlValueConverter.FromKdlOrThrow(
                    kdlValue,
                    map.Property.PropertyType,
                    map.KdlName,
                    map.TypeAnnotation
                );
                map.SetValue(instance, val);

                consumedPropKeys.Add(map.KdlName);
            }
        }
        foreach (var map in mapping.Properties.Where(m => m.IsDictionary))
        {
            var dict = EnsureInstance(instance, map) as IDictionary;
            bool isNamespaced = !string.IsNullOrWhiteSpace(map.KdlName);
            string prefix = isNamespaced ? $"{map.KdlName}:" : "";
            // Iterate through all actual properties in the KDL node
            foreach (var kdlProp in node.Properties)
            {
                string key = kdlProp.Key.Value;

                if (isNamespaced)
                {
                    // NAMESPACED logic: continue if it doesn't match our prefix
                    if (!key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        string dictKey = key.Substring(prefix.Length);
                        dict![dictKey] = KdlValueConverter.FromKdlOrThrow(
                            kdlProp.Value,
                            map.Property.PropertyType.GetGenericArguments()[1],
                            key
                        );
                    }
                }
                else
                {
                    // Greedy: take anything that wasn't matched by an explicit property in Pass 1
                    if (consumedPropKeys.Contains(key) || key.Contains(':'))
                    {
                        continue;
                    }
                    var valueType = map.Property.PropertyType.GetGenericArguments()[1];
                    dict![key] = KdlValueConverter.FromKdlOrThrow(kdlProp.Value, valueType, key);
                }
            }
        }
        if (node.Children != null)
        {
            // Mode B: Document Mode or Intrinsic Dictionary at root
            if (mapping.IsDictionary)
            {
                var explicitChildNames = mapping
                    .Children.Select(c => c.KdlName)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
                var dictionaryNodes = node.Children.Nodes.Where(n =>
                    !explicitChildNames.Contains(n.Name.Value)
                );

                PopulateDictionary(
                    (IDictionary)instance,
                    dictionaryNodes,
                    mapping.DictionaryKeyProperty!.PropertyType,
                    mapping.DictionaryValueProperty!.PropertyType
                );
            }
            MapChildren(node.Children.Nodes, instance, mapping);
        }
    }

    /// <summary>
    /// Maps child KDL nodes to properties marked with [KdlNode].
    /// </summary>
    private void MapChildren(List<KdlNode>? nodes, object instance, KdlTypeMapping mapping)
    {
        if (nodes is null || nodes.Count == 0)
            return;

        foreach (var map in mapping.Children)
        {
            List<KdlNode> matches = nodes
                .Where(n => n.Name.Value.Equals(map.KdlName, NodeNameComparison))
                .ToList();

            if (matches is null || matches.Count == 0)
                continue;

            List<KdlNode> nodesToProcess = map.IsFlattened
                ? matches
                : matches[^1].Children?.Nodes ?? [];

            if (map.IsDictionary)
            {
                var dict = EnsureInstance(instance, map) as IDictionary;
                PopulateDictionary(
                    dict!,
                    nodesToProcess,
                    map.DictionaryKeyProperty!.PropertyType,
                    map.DictionaryValueProperty!.PropertyType
                );
            }
            else if (map.IsCollection)
            {
                PopulateCollection(instance, nodesToProcess, map);
            }
            else
            {
                var last = matches.Last();
                object? value;

                if (map.Property.PropertyType.IsKdlScalar)
                {
                    var arg = last.Arg(0);
                    value =
                        arg != null
                            ? KdlValueConverter.FromKdlOrThrow(
                                arg,
                                map.Property.PropertyType,
                                last.Name.Value
                            )
                            : null;
                }
                else
                {
                    value = DeserializeObject(last, map.Property.PropertyType);
                }

                map.SetValue(instance, value);
            }
        }
    }

    private void PopulateCollection(
        object parentInstance,
        IEnumerable<KdlNode> nodes,
        KdlMemberMap map
    )
    {
        var list = CreateList(map.ElementType!);
        var elementMapping = KdlTypeMapping.For(map.ElementType!);

        foreach (var node in nodes)
        {
            // If the element name matches (or we are in a wrapped block), deserialize it
            object? item;
            if (map.ElementType!.IsKdlScalar)
            {
                var kdlVal = node.Arg(0);
                item =
                    kdlVal != null
                        ? KdlValueConverter.FromKdlOrThrow(
                            kdlVal,
                            map.ElementType!,
                            node.Name.Value
                        )
                        : null;
            }
            else
            {
                item = DeserializeObject(node, map.ElementType!);
            }

            if (item != null)
                list.Add(item);
        }

        map.SetValue(
            parentInstance,
            ConvertCollection(list, map.Property.PropertyType, map.ElementType!)
        );
    }

    private void PopulateDictionary(
        IDictionary dict,
        IEnumerable<KdlNode> nodes,
        Type keyType,
        Type valueType
    )
    {
        foreach (var node in nodes)
        {
            object key = Convert.ChangeType(node.Name.Value, keyType);
            object? value;

            if (valueType.IsKdlScalar)
            {
                var arg = node.Arg(0);
                value =
                    arg != null
                        ? KdlValueConverter.FromKdlOrThrow(arg, valueType, key.ToString()!)
                        : null;
            }
            else
            {
                value = DeserializeObject(node, valueType);
            }

            if (value != null)
                dict[key] = value;
        }
    }

    private object DeserializeObject(KdlNode node, Type type)
    {
        if (type.IsKdlScalar) { }
        var instance =
            Activator.CreateInstance(type)
            ?? throw new KuddleSerializationException(
                $"Failed to create instance of '{type.Name}'."
            );

        MapNodeToObject(node, instance, KdlTypeMapping.For(type));
        return instance;
    }

    private static void ValidateNodeName(KdlNode node, string nodeName)
    {
        if (!node.Name.Value.Equals(nodeName, NodeNameComparison))
        {
            throw new KuddleSerializationException(
                $"Expected node '{nodeName}', found '{node.Name.Value}'."
            );
        }
    }

    private object EnsureInstance(object parent, KdlMemberMap map)
    {
        var current = map.GetValue(parent);
        if (current != null)
            return current;

        var newInstance = Activator.CreateInstance(map.Property.PropertyType)!;
        map.SetValue(parent, newInstance);
        return newInstance;
    }

    private IList CreateList(Type elementType)
    {
        var listType = typeof(List<>).MakeGenericType(elementType);
        return (IList)Activator.CreateInstance(listType)!;
    }

    private object ConvertCollection(IList list, Type targetType, Type elementType)
    {
        if (targetType.IsArray)
        {
            var array = Array.CreateInstance(elementType, list.Count);
            list.CopyTo(array, 0);
            return array;
        }
        return list; // Assuming List<T> is compatible with target (IEnumerable/IReadOnlyList)
    }
}
