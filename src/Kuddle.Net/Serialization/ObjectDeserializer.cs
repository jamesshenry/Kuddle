using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        return worker.DeserializeRoot<T>(doc);
    }

    internal T DeserializeRoot<T>(KdlDocument doc)
        where T : new()
    {
        var metadata = KdlTypeInfo.For<T>();

        if (metadata.IsIEnumerable)
        {
            throw new KuddleSerializationException(
                $"Cannot deserialize to collection type '{metadata.Type.Name}'. Use DeserializeMany<T>() instead."
            );
        }

        if (metadata.IsStrictNode)
        {
            if (doc.Nodes.Count > 1)
                throw new KuddleSerializationException(
                    $"Expected exactly 1 root node, found {doc.Nodes.Count}."
                );

            var rootNode = doc.Nodes[0];
            ValidateNodeName(rootNode, metadata.NodeName);

            var instance = new T();
            MapNodeToObject(rootNode, instance, metadata);
            return instance;
        }
        else
        {
            // Document Mode: Root properties and children
            var instance = new T();
            MapChildNodes(doc.Nodes, instance, metadata);
            MapDictionaries(doc.Nodes, null, instance, metadata);
            return instance;
        }
    }

    internal static T DeserializeNode<T>(KdlNode node, KdlSerializerOptions? options)
        where T : new()
    {
        var worker = new ObjectDeserializer(options);
        var metadata = KdlTypeInfo.For<T>();
        ValidateNodeName(node, metadata.NodeName);

        var instance = new T();
        worker.MapNodeToObject(node, instance, metadata);
        return instance;
    }

    private object DeserializeObject(KdlNode node, Type type)
    {
        var instance =
            Activator.CreateInstance(type)
            ?? throw new KuddleSerializationException(
                $"Failed to create instance of '{type.Name}'."
            );

        var metadata = KdlTypeInfo.For(type);
        MapNodeToObject(node, instance, metadata);
        return instance;
    }

    /// <summary>
    /// Maps a KDL node's entries and children to an object instance.
    /// </summary>
    private void MapNodeToObject(KdlNode node, object instance, KdlTypeInfo metadata)
    {
        foreach (var mapping in metadata.ArgumentAttributes)
        {
            var argValue =
                node.Arg(mapping.ArgumentIndex)
                ?? throw new KuddleSerializationException(
                    $"Missing required argument at index {mapping.ArgumentIndex}."
                );
            SetPropertyValue(mapping.Property, instance, argValue, mapping.TypeAnnotation);
        }

        foreach (var mapping in metadata.Properties)
        {
            var kdlValue = node.Prop(mapping.Name);
            if (kdlValue != null)
            {
                SetPropertyValue(mapping.Property, instance, kdlValue, mapping.TypeAnnotation);
            }
        }

        MapChildNodes(node.Children?.Nodes, instance, metadata);

        MapDictionaries(node.Children?.Nodes, node, instance, metadata);

        if (metadata.IsDictionary && metadata.DictionaryDef != null)
        {
            var (keyType, valueType) = metadata.DictionaryDef;
            PopulateNodeDictionary(node.Children?.Nodes, (IDictionary)instance, keyType, valueType);
        }
    }

    /// <summary>
    /// Maps child KDL nodes to properties marked with [KdlNode].
    /// </summary>
    private void MapChildNodes(IReadOnlyList<KdlNode>? nodes, object instance, KdlTypeInfo metadata)
    {
        if (nodes is null || nodes.Count == 0)
            return;

        foreach (var mapping in metadata.Children)
        {
            var matches = nodes
                .Where(n => n.Name.Value.Equals(mapping.Name, NodeNameComparison))
                .ToList();

            if (matches.Count == 0)
                continue;

            var propType = mapping.Property.PropertyType;
            var propMeta = KdlTypeInfo.For(propType);

            if (propMeta.IsIEnumerable)
            {
                SetCollectionProperty(mapping.Property, instance, matches);
            }
            else
            {
                if (matches.Count > 1)
                    throw new KuddleSerializationException(
                        $"Expected single node '{mapping.Name}', found {matches.Count}."
                    );

                var node = matches[0];
                object value;

                if (propMeta.IsComplexType)
                {
                    value = DeserializeObject(node, propType);
                }
                else
                {
                    var arg = node.Arg(0);
                    if (arg is null)
                        continue;
                    value = KdlValueConverter.FromKdlOrThrow(
                        arg,
                        propType,
                        mapping.Name,
                        mapping.TypeAnnotation
                    );
                }

                mapping.Property.SetValue(instance, value);
            }
        }
    }

    private void MapDictionaries(
        List<KdlNode>? nodes,
        KdlNode? parentNode,
        object instance,
        KdlTypeInfo metadata
    )
    {
        if (metadata.Dictionaries.Count == 0)
            return;

        foreach (var member in metadata.Dictionaries)
        {
            var propMeta = KdlTypeInfo.For(member.Property.PropertyType);
            if (!propMeta.IsDictionary || propMeta.DictionaryDef is null)
                continue;

            var (keyType, valueType) = propMeta.DictionaryDef;
            var dict = GetOrCreateDictionary(instance, member);
            if (dict is null)
                continue;

            if (member.IsNodeDictionary && nodes != null)
            {
                var container = nodes.LastOrDefault(n =>
                    n.Name.Value.Equals(member.Name, NodeNameComparison)
                );
                if (container?.Children?.Nodes != null)
                {
                    PopulateNodeDictionary(container.Children.Nodes, dict, keyType, valueType);
                }
            }
        }
        static IDictionary? GetOrCreateDictionary(object instance, KdlMemberInfo member)
        {
            var dict = (IDictionary?)member.Property.GetValue(instance);
            if (dict is null)
            {
                dict = (IDictionary?)Activator.CreateInstance(member.Property.PropertyType);
                member.Property.SetValue(instance, dict);
            }

            return dict;
        }
    }

    private void PopulateNodeDictionary(
        IEnumerable<KdlNode>? children,
        IDictionary dict,
        Type keyType,
        Type valueType
    )
    {
        if (children is null)
            return;

        foreach (var child in children)
        {
            object key = Convert.ChangeType(child.Name.Value, keyType);
            object value;

            if (!valueType.IsComplexType)
            {
                // Scalar: Arg 0
                var arg = child.Arg(0);
                if (arg is null)
                    continue;
                value = KdlValueConverter.FromKdlOrThrow(arg, valueType, $"Dictionary Value {key}");
            }
            else
            {
                value = DeserializeObject(child, valueType);
            }

            if (dict.Contains(key))
                throw new KuddleSerializationException(
                    $"Duplicate dictionary key detected: '{key}'"
                );

            dict.Add(key, value);
        }
    }

    private void SetPropertyValue(
        PropertyInfo property,
        object instance,
        KdlValue kdlValue,
        string? expectedTypeAnnotation = null
    )
    {
        var result = KdlValueConverter.FromKdlOrThrow(
            kdlValue,
            property.PropertyType,
            $"Property: {property.DeclaringType?.Name}.{property.Name}",
            expectedTypeAnnotation
        );

        property.SetValue(instance, result);
    }

    private void SetCollectionProperty(PropertyInfo property, object instance, List<KdlNode> nodes)
    {
        var elementType = property.PropertyType.GetCollectionElementType();

        var listType = typeof(List<>).MakeGenericType(elementType);
        var list = (IList)Activator.CreateInstance(listType)!;

        var elementMetadata = KdlTypeInfo.For(elementType);

        foreach (var node in nodes)
        {
            object element;
            if (elementMetadata.IsComplexType)
            {
                element = DeserializeObject(node, elementType);
            }
            else
            {
                var arg = node.Arg(0);
                if (arg is null)
                    continue;
                element = KdlValueConverter.FromKdlOrThrow(arg, elementType, "List Element");
            }
            list.Add(element);
        }

        var finalValue = ConvertToTargetCollectionType(property.PropertyType, elementType, list);
        property.SetValue(instance, finalValue);
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

    private static object ConvertToTargetCollectionType(
        Type targetType,
        Type elementType,
        IList list
    )
    {
        if (targetType.IsArray)
        {
            var array = Array.CreateInstance(elementType, list.Count);
            list.CopyTo(array, 0);
            return array;
        }

        if (targetType.IsAssignableFrom(list.GetType()))
        {
            return list;
        }

        return list;
    }
}
