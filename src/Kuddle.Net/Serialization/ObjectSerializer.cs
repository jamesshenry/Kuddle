using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kuddle.AST;

namespace Kuddle.Serialization;

internal class ObjectSerializer
{
    private readonly KdlSerializerOptions _options;

    public ObjectSerializer(KdlSerializerOptions? options = null)
    {
        _options = options ?? KdlSerializerOptions.Default;
    }

    internal static KdlDocument SerializeDocument<T>(T? instance, KdlSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);

        var type = typeof(T);
        var worker = new ObjectSerializer(options);

        if (type.IsKdlScalar)
        {
            throw new KuddleSerializationException(
                $"Cannot serialize primitive type '{type.Name}'. Only complex types are supported."
            );
        }

        var doc = new KdlDocument();

        // If the root object itself is a collection, we treat every item as a top-level node.
        if (typeof(T).IsIEnumerable && instance is IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                if (item != null)
                    doc.Nodes.Add(worker.SerializeObject(item));
            }
        }
        else if (options.RootMapping == KdlRootMapping.AsDocument)
        {
            var rootNode = worker.SerializeObject(instance);

            // 2. Promote Properties to top-level Nodes
            // In KDL config, a "Property" at root is a node with one argument: title "example"
            foreach (var entry in rootNode.Entries)
            {
                if (entry is KdlProperty prop)
                {
                    doc.Nodes.Add(
                        new KdlNode(prop.Key) { Entries = [new KdlArgument(prop.Value)] }
                    );
                }
                else if (entry is KdlArgument arg)
                {
                    // Map positional arguments to anonymous nodes
                    doc.Nodes.Add(new KdlNode(KdlValue.From("-")) { Entries = [arg] });
                }
            }
            if (rootNode.Children != null)
            {
                doc.Nodes.AddRange(rootNode.Children.Nodes);
            }
        }
        else
        {
            doc.Nodes.Add(worker.SerializeObject(instance));
        }

        return doc;
    }

    private KdlNode SerializeObject(object instance, string? overrideNodeName = null)
    {
        var mapping = KdlTypeMapping.For(instance.GetType());
        var node = new KdlNode(KdlValue.From(overrideNodeName ?? mapping.NodeName));

        foreach (var map in mapping.Arguments)
        {
            var val = KdlValueConverter.ToKdlOrThrow(map.GetValue(instance), map.TypeAnnotation);
            node.Entries.Add(new KdlArgument(val));
        }

        foreach (var map in mapping.Properties)
        {
            var raw = map.GetValue(instance);
            if (raw == null && _options.IgnoreNullValues)
                continue;

            if (map.IsDictionary)
            {
                string prefix = string.IsNullOrEmpty(map.KdlName) ? "" : $"{map.KdlName}:";

                var dict = raw as IEnumerable;
                foreach (var item in dict!)
                {
                    var k = map.DictionaryKeyProperty?.GetValue(item);
                    var v = map.DictionaryValueProperty?.GetValue(item);
                    if (k == null)
                        continue;
                    node.Entries.Add(
                        new KdlProperty(
                            KdlValue.From($"{prefix}{k}"),
                            KdlValueConverter.ToKdlOrThrow(v)
                        )
                    );
                }
            }
            else
            {
                var val = KdlValueConverter.ToKdlOrThrow(raw, map.TypeAnnotation);
                node.Entries.Add(new KdlProperty(KdlValue.From(map.KdlName), val));
            }
        }

        var childNodes = new List<KdlNode>();
        foreach (var map in mapping.Children)
        {
            var childData = map.GetValue(instance);
            if (childData is null)
                continue;

            if (map.IsDictionary && childData is IEnumerable mapDict)
            {
                var items = SerializeDictionary(
                    mapDict,
                    map.DictionaryKeyProperty,
                    map.DictionaryValueProperty
                );
                if (map.IsFlattened)
                {
                    childNodes.AddRange(items);
                }
                else
                {
                    var container = new KdlNode(KdlValue.From(map.KdlName))
                    {
                        Children = new KdlBlock { Nodes = items.ToList() },
                    };
                    childNodes.Add(container);
                }
            }
            else if (map.IsCollection && childData is IEnumerable childCol)
            {
                var items = SerializeCollection(childCol, map);

                if (map.IsFlattened)
                {
                    childNodes.AddRange(items);
                }
                else
                {
                    var container = new KdlNode(KdlValue.From(map.KdlName))
                    {
                        Children = new KdlBlock { Nodes = items.ToList() },
                    };
                    childNodes.Add(container);
                }
            }
            else
            {
                childNodes.Add(SerializeObject(childData, map.KdlName));
            }
        }
        if (mapping.IsDictionary && instance is IEnumerable enumerable)
        {
            var items = SerializeDictionary(
                enumerable,
                mapping.DictionaryKeyProperty,
                mapping.DictionaryValueProperty
            );
            childNodes.AddRange(items);
        }

        if (childNodes.Count > 0)
        {
            node = node with { Children = new KdlBlock { Nodes = childNodes } };
        }
        return node;
    }

    private IEnumerable<KdlNode> SerializeCollection(IEnumerable enumerable, KdlMemberMap map)
    {
        foreach (var item in enumerable)
        {
            if (item is null)
                continue;

            string itemName = map.IsFlattened
                ? map.KdlName
                : map.ElementName ?? GetDefaultNodeName(item);

            yield return MapToNode(item, itemName, map.TypeAnnotation);
        }
    }

    private string GetDefaultNodeName(object item)
    {
        if (item.GetType().IsKdlScalar)
            return "-";

        return _options.SimpleCollectionNodeNames
            ? "-"
            : KdlTypeMapping.For(item.GetType()).NodeName;
    }

    private KdlNode MapToNode(object item, string kdlName, string? typeAnnotation)
    {
        if (item.GetType().IsKdlScalar)
        {
            var val = KdlValueConverter.ToKdlOrThrow(item, typeAnnotation);
            return new KdlNode(KdlValue.From(kdlName)) { Entries = [new KdlArgument(val)] };
        }
        else
        {
            return SerializeObject(item, kdlName);
        }
    }

    private IEnumerable<KdlNode> SerializeDictionary(
        IEnumerable dict,
        PropertyInfo? keyProp,
        PropertyInfo? valProp,
        string? typeAnno = null
    )
    {
        foreach (var item in dict)
        {
            var key = keyProp?.GetValue(item);
            var val = valProp?.GetValue(item);
            if (key == null || val == null)
                continue;

            yield return MapToNode(val, key.ToString()!, typeAnno);
        }
    }
}
