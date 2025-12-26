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

    internal static KdlDocument SerializeDocument<T>(T? instance, KdlSerializerOptions? options)
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
                var container = new KdlNode(KdlValue.From(map.KdlName));
                var items = SerializeDictionary(
                    mapDict,
                    map.DictionaryKeyProperty,
                    map.DictionaryValueProperty
                );
                container = container with { Children = new KdlBlock { Nodes = items.ToList() } };
                childNodes.Add(container);
            }
            else if (map.IsCollection && childData is IEnumerable childCol)
            {
                childNodes.AddRange(SerializeCollection(childCol, map));
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

            yield return MapToNode(item, map.KdlName, map.TypeAnnotation);
        }
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
