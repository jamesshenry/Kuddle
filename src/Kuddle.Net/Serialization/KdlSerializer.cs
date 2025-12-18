using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kuddle.AST;
using Kuddle.Extensions;

namespace Kuddle.Serialization;

/// <summary>
/// Serializes and deserializes C# objects to/from KDL format.
/// </summary>
public static class KdlSerializer
{
    #region Deserialization

    /// <summary>
    /// Deserializes a KDL document containing multiple nodes of type T.
    /// </summary>
    public static IEnumerable<T> DeserializeMany<T>(
        string text,
        KdlSerializerOptions? options = null
    )
        where T : new()
    {
        var doc = KdlReader.Read(text);
        var metadata = TypeMetadata.For<T>();

        foreach (var node in doc.Nodes)
        {
            if (!node.Name.Value.Equals(metadata.NodeName, StringComparison.OrdinalIgnoreCase))
            {
                throw new KuddleSerializationException(
                    $"Expected node '{metadata.NodeName}', found '{node.Name.Value}'."
                );
            }

            var item = new T();
            MapNodeToObject(node, item, metadata);
            yield return item;
        }
    }

    /// <summary>
    /// Deserializes a KDL document to a single object of type T.
    /// </summary>
    public static T Deserialize<T>(string text, KdlSerializerOptions? options = null)
        where T : new()
    {
        var document = KdlReader.Read(text);
        var metadata = TypeMetadata.For<T>();

        // Reject dictionary types
        if (metadata.IsDictionary)
        {
            throw new KuddleSerializationException(
                $"Dictionary deserialization is not supported. Type: {metadata.Type.Name}"
            );
        }

        // Reject IEnumerable types (use DeserializeMany instead)
        if (metadata.IsIEnumerable)
        {
            throw new KuddleSerializationException(
                $"Cannot deserialize to collection type '{metadata.Type.Name}'. Use DeserializeMany<T>() instead."
            );
        }

        if (metadata.IsNodeDefinition)
        {
            // Type maps to a single KDL node
            if (document.Nodes.Count != 1)
            {
                throw new KuddleSerializationException(
                    $"Expected exactly 1 root node for type '{typeof(T).Name}', found {document.Nodes.Count}."
                );
            }

            var rootNode = document.Nodes[0];
            if (!rootNode.Name.Value.Equals(metadata.NodeName, StringComparison.OrdinalIgnoreCase))
            {
                throw new KuddleSerializationException(
                    $"Expected node '{metadata.NodeName}', found '{rootNode.Name.Value}'."
                );
            }

            var instance = new T();
            MapNodeToObject(rootNode, instance, metadata);
            return instance;
        }
        else
        {
            // Type maps to a document with child nodes as properties
            var instance = new T();
            MapChildNodes(document.Nodes, instance, metadata);
            return instance;
        }
    }

    /// <summary>
    /// Maps a KDL node's entries and children to an object instance.
    /// </summary>
    private static void MapNodeToObject(KdlNode node, object instance, TypeMetadata metadata)
    {
        // Map arguments
        foreach (var mapping in metadata.ArgumentAttributes)
        {
            var argValue = node.Arg(mapping.ArgumentIndex);
            if (argValue is null)
            {
                throw new KuddleSerializationException(
                    $"Missing required argument at index {mapping.ArgumentIndex}'."
                );
            }

            SetPropertyValue(mapping.Property, instance, argValue, mapping.TypeAnnotation);
        }

        // Map properties
        foreach (var mapping in metadata.Properties)
        {
            var propKey = mapping.GetPropertyKey();
            var kdlValue = node.Prop(propKey);

            if (kdlValue is null)
            {
                continue; // Optional property, use default
            }

            SetPropertyValue(mapping.Property, instance, kdlValue, mapping.TypeAnnotation);
        }

        // Map child nodes
        MapChildNodes(node.Children?.Nodes, instance, metadata);
    }

    /// <summary>
    /// Maps child KDL nodes to properties marked with [KdlNode].
    /// </summary>
    private static void MapChildNodes(
        IReadOnlyList<KdlNode>? nodes,
        object instance,
        TypeMetadata metadata
    )
    {
        if (nodes is null || nodes.Count == 0)
        {
            return;
        }

        foreach (var mapping in metadata.Children)
        {
            var nodeName = mapping.GetChildNodeName();
            var matchingNodes = nodes
                .Where(n => n.Name.Value.Equals(nodeName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matchingNodes.Count == 0)
            {
                continue;
            }

            var propType = mapping.Property.PropertyType;
            var propMeta = TypeMetadata.For(propType);

            if (propMeta.IsIEnumerable)
            {
                SetCollectionProperty(mapping.Property, instance, matchingNodes);
            }
            else if (propMeta.IsComplexType)
            {
                if (matchingNodes.Count > 1)
                {
                    throw new KuddleSerializationException(
                        $"Expected single node '{nodeName}' for property '{mapping.Property.Name}', found {matchingNodes.Count}."
                    );
                }

                SetComplexProperty(mapping.Property, instance, matchingNodes[0]);
            }
            else
            {
                // Scalar property - extract from first argument
                if (matchingNodes.Count > 1)
                {
                    throw new KuddleSerializationException(
                        $"Expected single node '{nodeName}' for scalar property '{mapping.Property.Name}', found {matchingNodes.Count}."
                    );
                }

                var argValue = matchingNodes[0].Arg(0);
                if (argValue is not null)
                {
                    SetPropertyValue(mapping.Property, instance, argValue, mapping.TypeAnnotation);
                }
            }
        }
    }

    private static void SetPropertyValue(
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

    private static void SetComplexProperty(PropertyInfo property, object instance, KdlNode node)
    {
        var childInstance =
            Activator.CreateInstance(property.PropertyType)
            ?? throw new KuddleSerializationException(
                $"Failed to create instance of '{property.PropertyType.Name}' for property '{property.Name}'."
            );

        var childMetadata = TypeMetadata.For(property.PropertyType);
        MapNodeToObject(node, childInstance, childMetadata);

        property.SetValue(instance, childInstance);
    }

    private static void SetCollectionProperty(
        PropertyInfo property,
        object instance,
        List<KdlNode> nodes
    )
    {
        var elementType = GetCollectionElementType(property.PropertyType);
        var metadata = TypeMetadata.For(property.PropertyType);

        if (!metadata.IsComplexType)
        {
            throw new KuddleSerializationException(
                $"Collection element type '{elementType.Name}' must be a complex type for property '{property.Name}'."
            );
        }

        var listType = typeof(List<>).MakeGenericType(elementType);
        var list = (IList)Activator.CreateInstance(listType)!;

        var elementMetadata = TypeMetadata.For(elementType);

        foreach (var node in nodes)
        {
            var element =
                Activator.CreateInstance(elementType)
                ?? throw new KuddleSerializationException(
                    $"Failed to create instance of '{elementType.Name}'."
                );

            MapNodeToObject(node, element, elementMetadata);
            list.Add(element);
        }

        var finalValue = ConvertToTargetCollectionType(property.PropertyType, elementType, list);
        property.SetValue(instance, finalValue);
    }

    #endregion

    #region Serialization

    /// <summary>
    /// Serializes an object to a KDL string.
    /// </summary>
    public static string Serialize<T>(T instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        var type = typeof(T);
        var metadata = TypeMetadata.For<T>();

        if (!metadata.IsComplexType)
        {
            throw new KuddleSerializationException(
                $"Cannot serialize primitive type '{type.Name}'. Only complex types are supported."
            );
        }

        var doc = new KdlDocument();

        if (metadata.IsDictionary)
        {
            throw new NotSupportedException("Dictionary serialization is not yet supported.");
        }

        if (metadata.IsIEnumerable)
        {
            var nodes = SerializeCollection((IEnumerable)instance);
            doc.Nodes.AddRange(nodes);
        }
        else
        {
            var node = SerializeToNode(instance);
            doc.Nodes.Add(node);
        }

        return KdlWriter.Write(doc);
    }

    private static KdlNode SerializeToNode(object instance, string? overrideNodeName = null)
    {
        var type = instance.GetType();
        var metadata = TypeMetadata.For(type);
        var nodeName = overrideNodeName ?? metadata.NodeName;

        var entries = new List<KdlEntry>();

        // Serialize arguments (in order)
        foreach (var mapping in metadata.ArgumentAttributes)
        {
            var value = mapping.Property.GetValue(instance);
            var typeAnnotation = mapping.TypeAnnotation;
            var kdlValue = KdlValueConverter.ToKdlOrThrow(
                value,
                $"Argument property: {mapping.Property.Name}",
                typeAnnotation
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

            var key = mapping.GetPropertyKey();
            entries.Add(new KdlProperty(KdlValue.From(key), kdlValue));
        }

        // Serialize children
        KdlBlock? childBlock = null;

        foreach (var mapping in metadata.Children)
        {
            var propValue = mapping.Property.GetValue(instance);

            if (propValue is null)
            {
                continue;
            }

            childBlock ??= new KdlBlock();
            var childNodeName = mapping.GetChildNodeName();

            var propType = mapping.Property.PropertyType;
            var childMeta = TypeMetadata.For(propType);

            if (childMeta.IsIEnumerable)
            {
                var childNodes = SerializeCollection((IEnumerable)propValue, childNodeName);
                childBlock.Nodes.AddRange(childNodes);
            }
            else if (childMeta.IsComplexType)
            {
                var childNode = SerializeToNode(propValue, childNodeName);
                childBlock.Nodes.Add(childNode);
            }
            else
            {
                // Scalar value as a child node with single argument
                var typeAnnotation = mapping.TypeAnnotation;
                var kdlValue = KdlValueConverter.ToKdlOrThrow(
                    propValue,
                    $"Child scalar property: {mapping.Property.Name}",
                    typeAnnotation
                );

                var scalarNode = new KdlNode(KdlValue.From(childNodeName))
                {
                    Entries = [new KdlArgument(kdlValue)],
                };
                childBlock.Nodes.Add(scalarNode);
            }
        }

        return new KdlNode(KdlValue.From(nodeName))
        {
            Entries = entries,
            Children = childBlock?.Nodes.Count > 0 ? childBlock : null,
        };
    }

    private static IEnumerable<KdlNode> SerializeCollection(
        IEnumerable collection,
        string? overrideNodeName = null
    )
    {
        foreach (var item in collection)
        {
            if (item is null)
            {
                continue;
            }

            yield return SerializeToNode(item, overrideNodeName);
        }
    }

    #endregion

    #region Helpers

    private static Type GetCollectionElementType(Type collectionType)
    {
        if (collectionType.IsArray)
        {
            return collectionType.GetElementType()!;
        }

        if (collectionType.IsGenericType)
        {
            return collectionType.GetGenericArguments()[0];
        }

        throw new KuddleSerializationException(
            $"Unsupported collection type '{collectionType.FullName}'."
        );
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

    #endregion
}

/// <summary>
/// Options for KDL serialization (reserved for future use).
/// </summary>
public record KdlSerializerOptions { }
