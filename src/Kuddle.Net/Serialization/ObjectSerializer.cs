using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks.Dataflow;
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

    public static KdlNode SerializeNode(object instance, KdlSerializerOptions? options)
    {
        var worker = new ObjectSerializer(options);
        return worker.SerializeObject(instance);
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

            var val = KdlValueConverter.ToKdlOrThrow(raw, map.TypeAnnotation);
            node.Entries.Add(new KdlProperty(KdlValue.From(map.KdlName), val));
        }

        var childNodes = new List<KdlNode>();
        if (mapping.Children.Count > 0)
        {
            foreach (var map in mapping.Children)
            {
                var childData = map.GetValue(instance);
                if (childData is null)
                    continue;

                if (map.IsDictionary)
                {
                    // For dictionaries, we treat the Dictionary Keys as the Node Names
                    var container = new KdlNode(KdlValue.From(map.KdlName));
                    var items = SerializeDictionary((IEnumerable)childData, map);
                    container = container with
                    {
                        Children = new KdlBlock { Nodes = items.ToList() },
                    };
                    childNodes.Add(container);
                    // childNodes.AddRange(SerializeDictionary((IDictionary)childData, map));
                }
                else if (map.IsCollection)
                {
                    // For collections, we serialize each item as a child node
                    childNodes.AddRange(SerializeCollection((IEnumerable)childData, map));
                }
                else
                {
                    // Single complex object
                    childNodes.Add(SerializeObject(childData, map.KdlName));
                }
            }
        }

        // if (mapping.IsDictionary && instance is IEnumerable enumerable)
        // {
        //     childNodes.AddRange(
        //         SerializeDictionaryContent(
        //             enumerable,
        //             mapping.DictionaryKeyProperty,
        //             mapping.DictionaryValueProperty,
        //             null
        //         )
        //     );
        // }

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

    private IEnumerable<KdlNode> SerializeDictionary(IEnumerable dict, KdlMemberMap map)
    {
        foreach (var item in dict)
        {
            var key = map.DictionaryKeyProperty!.GetValue(item);
            var val = map.DictionaryValueProperty!.GetValue(item);
            if (key == null || val == null)
                continue;

            yield return MapToNode(val!, key.ToString()!, map.TypeAnnotation);
        }
    }

    private IEnumerable<KdlNode> SerializeDictionaryContent(
        IEnumerable dict,
        PropertyInfo? keyProp,
        PropertyInfo? valProp,
        string? typeAnno
    )
    {
        foreach (var item in dict)
        {
            var key = keyProp?.GetValue(item);
            var val = valProp?.GetValue(item);
            if (key == null || val == null)
                continue;

            // MapToNode handles the recursion:
            // If 'val' is a dictionary, it calls SerializeObject, which triggers step (B) above.
            yield return MapToNode(val, key.ToString()!, typeAnno);
        }
    }
}
