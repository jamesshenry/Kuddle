using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kuddle.AST;

namespace Kuddle.Serialization;

internal class ObjectSerializer
{
    private readonly KdlSerializerOptions _options;

    public ObjectSerializer(KdlSerializerOptions? options = null)
    {
        _options = options ?? KdlSerializerOptions.Default;
    }

    internal static KdlDocument SerializeDocument<T>(T? instance, KdlSerializerOptions? options)
    {
        ArgumentNullException.ThrowIfNull(instance);

        var type = typeof(T);
        var worker = new ObjectSerializer(options);
        var metadata = KdlTypeInfo.For<T>();

        if (!metadata.IsComplexType && !metadata.IsDictionary)
        {
            throw new KuddleSerializationException(
                $"Cannot serialize primitive type '{type.Name}'. Only complex types are supported."
            );
        }
        var doc = new KdlDocument();

        if (metadata.IsIEnumerable)
        {
            var nodes = worker.SerializeCollection((IEnumerable)instance);
            doc.Nodes.AddRange(nodes);
        }
        else
        {
            var node = worker.SerializeToNode(instance);
            doc.Nodes.Add(node);
        }

        return doc;
    }

    public static KdlNode SerializeNode(object instance, KdlSerializerOptions? options)
    {
        var worker = new ObjectSerializer(options);
        return worker.SerializeToNode(instance);
    }

    private IEnumerable<KdlNode> SerializeCollection(
        IEnumerable collection,
        string? overrideNodeName = null
    )
    {
        foreach (var item in collection)
        {
            if (item is null)
                continue;

            yield return SerializeToNode(item, overrideNodeName);
        }
    }

    private KdlNode SerializeToNode(object instance, string? overrideNodeName = null)
    {
        var metadata = KdlTypeInfo.For(instance.GetType());
        var nodeName = overrideNodeName ?? metadata.NodeName;

        var entries = new List<KdlEntry>();

        // Serialize arguments (in order)
        foreach (var mapping in metadata.ArgumentAttributes)
        {
            var value = mapping.Property.GetValue(instance);
            var kdlValue = KdlValueConverter.ToKdlOrThrow(
                value,
                $"Argument property: {mapping.Property.Name}",
                mapping.TypeAnnotation
            );

            entries.Add(new KdlArgument(kdlValue));
        }

        // Serialize properties
        foreach (var mapping in metadata.Properties)
        {
            var value = mapping.Property.GetValue(instance);
            var typeAnnotation = mapping.TypeAnnotation;

            if (!KdlValueConverter.TryToKdl(value, out var kdlValue, typeAnnotation))
            {
                continue; // Skip properties that can't be converted
            }

            entries.Add(new KdlProperty(KdlValue.From(mapping.Name), kdlValue));
        }

        foreach (var dictMap in metadata.Dictionaries.Where(m => m.IsPropertyDictionary))
        {
            var dict = (IDictionary?)dictMap.Property.GetValue(instance);
            if (dict is null)
                continue;

            foreach (DictionaryEntry entry in dict)
            {
                var keyStr = entry.Key.ToString()!;
                if (entry.Value != null && KdlValueConverter.TryToKdl(entry.Value, out var val))
                {
                    entries.Add(new KdlProperty(KdlValue.From(keyStr), val));
                }
            }
        }
        // Serialize children
        KdlBlock? childBlock = null;

        void AddChild(KdlNode child)
        {
            childBlock ??= new KdlBlock();
            childBlock.Nodes.Add(child);
        }

        foreach (var mapping in metadata.Children)
        {
            var propValue = mapping.Property.GetValue(instance);

            if (propValue is null)
                continue;

            var propType = mapping.Property.PropertyType;
            var childMeta = KdlTypeInfo.For(propType);

            if (childMeta.IsIEnumerable)
            {
                var childNodes = SerializeCollection((IEnumerable)propValue, mapping.Name);
                foreach (var child in childNodes)
                {
                    AddChild(child);
                }
            }
            else if (childMeta.IsComplexType)
            {
                AddChild(SerializeToNode(propValue, mapping.Name));
            }
            else
            {
                var kdlValue = KdlValueConverter.ToKdlOrThrow(
                    propValue,
                    $"Child scalar property: {mapping.Property.Name}",
                    mapping.TypeAnnotation
                );

                var scalarNode = new KdlNode(KdlValue.From(mapping.Name))
                {
                    Entries = [new KdlArgument(kdlValue)],
                };
                AddChild(scalarNode);
            }
        }
        foreach (var dictMap in metadata.Dictionaries)
        {
            if (dictMap.IsPropertyDictionary)
                continue; // Handled above

            var dict = (IDictionary?)dictMap.Property.GetValue(instance);
            if (dict is null || dict.Count == 0)
                continue;

            var (_, valueType) = KdlTypeInfo.For(dictMap.Property.PropertyType).DictionaryDef!;

            if (dictMap.IsNodeDictionary)
            {
                // Strategy: Create a container node, put entries inside
                // themes { dark { ... } }
                var container = new KdlNode(KdlValue.From(dictMap.Name));
                var nodes = new List<KdlNode>();
                SerializeDictionaryEntries(nodes, dict, valueType);

                if (nodes.Count > 0)
                {
                    container = container with { Children = new KdlBlock() { Nodes = nodes } };
                    AddChild(container);
                }
            }
            else if (dictMap.IsKeyedNodeCollection)
            {
                // Strategy: Flatten entries as children
                // db "A" { ... }
                // db "B" { ... }
                foreach (var value in dict.Values)
                {
                    if (value is null)
                        continue;
                    // For KeyedNodes, the Object contains the Key, so we just serialize the Object
                    // with the forced Node Name from the attribute.
                    AddChild(SerializeToNode(value, dictMap.Name));
                }
            }
        }

        foreach (var mapping in metadata.Collections)
        {
            var val = mapping.Property.GetValue(instance);
            if (val is null)
                continue;

            var collection = (IEnumerable)val;

            // 1. Create the Container Node
            var container = new KdlNode(KdlValue.From(mapping.Name));
            var nodes = new List<KdlNode>();
            // 2. Determine Item Name
            var itemType = mapping.Property.PropertyType.GetCollectionElementType();
            var itemMeta = KdlTypeInfo.For(itemType);
            var targetName = mapping.CollectionElementName ?? itemMeta.NodeName;

            // 3. Serialize Items as children of the Container
            foreach (var item in collection)
            {
                if (item is null)
                    continue;
                // Recursively serialize, forcing the name to "event"
                nodes.Add(SerializeToNode(item, targetName));
            }

            // 4. Attach
            if (nodes.Count > 0)
            {
                container = container with { Children = new KdlBlock() { Nodes = nodes } };
                AddChild(container);
            }
        }

        if (
            metadata.IsDictionary
            && metadata.DictionaryDef != null
            && instance is IDictionary selfDict
        )
        {
            childBlock ??= new KdlBlock();

            SerializeDictionaryEntries(
                childBlock.Nodes,
                selfDict,
                metadata.DictionaryDef.ValueType
            );
        }
        return new KdlNode(KdlValue.From(nodeName))
        {
            Entries = entries,
            Children = childBlock?.Nodes.Count > 0 ? childBlock : null,
        };
    }

    /// <summary>
    /// Helper to convert Dictionary entries into KDL Nodes.
    /// Used by [KdlNodeDictionary] and Implicit Dictionary logic.
    /// </summary>
    private void SerializeDictionaryEntries(
        List<KdlNode> targetList,
        IDictionary dict,
        Type valueType
    )
    {
        var isScalar = valueType.IsKdlScalar;

        foreach (DictionaryEntry entry in dict)
        {
            var keyStr = entry.Key.ToString();
            if (string.IsNullOrEmpty(keyStr) || entry.Value is null)
                continue;

            if (isScalar)
            {
                // Scalar: NodeName=Key, Arg0=Value
                // e.g. timeout 5000
                if (KdlValueConverter.TryToKdl(entry.Value, out var kdlVal, null))
                {
                    targetList.Add(
                        new KdlNode(KdlValue.From(keyStr)) { Entries = [new KdlArgument(kdlVal)] }
                    );
                }
            }
            else
            {
                // Complex: NodeName=Key, Body=Object
                // e.g. dark-mode { ... }
                // We recurse SerializeToNode, but we override the name to be the Key
                targetList.Add(SerializeToNode(entry.Value, overrideNodeName: keyStr));
            }
        }
    }
}
