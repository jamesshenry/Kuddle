using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kuddle.AST;
using Kuddle.Extensions;

namespace Kuddle.Serialization;

public static class KdlSerializer
{
    public static IEnumerable<T> DeserializeMany<T>(
        string text,
        KdlSerializerOptions? options = null
    )
        where T : new()
    {
        var doc = KdlReader.Read(text);
        var list = new List<T>(doc.Nodes.Count);

        var expectedName = GetExpectedNodeName(typeof(T));

        foreach (var node in doc.Nodes)
        {
            if (expectedName != null && node.Name.Value != expectedName)
                throw new KuddleSerializationException(
                    $"Expected node '{expectedName}', found '{node.Name.Value}'"
                );

            var item = new T();
            MapToInstance(node, item);
            list.Add(item);
        }

        return list;
    }

    private static string? GetExpectedNodeName(Type type)
    {
        var kdlTypeAttr = type.GetCustomAttribute<KdlTypeAttribute>();

        if (kdlTypeAttr is null)
        {
            return type.Name.ToLowerInvariant();
        }
        else
        {
            return kdlTypeAttr.Name;
        }
    }

    public static T Deserialize<T>(string text, KdlSerializerOptions? options = null)
        where T : new()
    {
        var document = KdlReader.Read(text);
        var type = typeof(T);

        if (type.IsNodeDefinition)
        {
            if (document.Nodes.Count != 1)
            {
                throw new KuddleSerializationException(
                    "Kdl document must have a single root node to map to an instance T"
                );
            }

            var rootNode = document.Nodes[0];
            var expectedName = GetExpectedNodeName(type);

            if (
                expectedName != null
                && !rootNode.Name.Value.Equals(expectedName, StringComparison.OrdinalIgnoreCase)
            )
            {
                throw new KuddleSerializationException(
                    $"Node name '{rootNode.Name.Value}' does not match target type name '{typeof(T).Name}'."
                );
            }
            var instance = new T();
            MapToInstance(rootNode, instance);
            return instance;
        }
        else
        {
            var instance = new T();
            MapNodesToProperties(document.Nodes, instance);
            return instance;
        }
    }

    private static void MapNodesToProperties<T>(List<KdlNode> nodes, T instance)
    {
        var props = instance!
            .GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite && p.GetCustomAttribute<KdlIgnoreAttribute>() == null);

        foreach (var prop in props)
        {
            var nodeName = prop.GetCustomAttribute<KdlNodeAttribute>()?.Name;

            if (nodeName == null)
                continue;

            var matchingNodes = nodes
                .Where(n => n.Name.Value.Equals(nodeName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matchingNodes.Count == 0)
                continue;

            if (prop.PropertyType.IsIEnumerable)
            {
                SetCollectionPropValue(prop, instance, matchingNodes);
            }
            else
            {
                if (matchingNodes.Count > 1)
                    throw new KuddleSerializationException(
                        $"Property '{prop.Name}' expects a single node named '{nodeName}', but found {matchingNodes.Count}."
                    );

                SetSingleComplexPropValue(prop, instance, matchingNodes[0]);
            }
        }
    }

    private static void SetSingleComplexPropValue(PropertyInfo prop, object instance, KdlNode node)
    {
        var type = prop.PropertyType;
        if (!type.IsComplexType)
        {
            throw new KuddleSerializationException(
                $"Property '{prop.Name}' matches a Node, so it must be a complex class."
            );
        }

        var childInstance =
            Activator.CreateInstance(type) ?? throw new Exception($"Could not create {type}");

        MapToInstance(node, childInstance);

        prop.SetValue(instance, childInstance);
    }

    private static void MapToInstance<T>(KdlNode node, T instance)
    {
        var rootType = typeof(T);

        var props = instance!
            .GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite && p.GetCustomAttribute<KdlIgnoreAttribute>() == null);

        foreach (var prop in props)
        {
            var argAttr = prop.GetCustomAttribute<KdlArgumentAttribute>();
            if (argAttr is not null)
            {
                if (prop.PropertyType.IsComplexType)
                    throw new KuddleSerializationException();

                var targetArg =
                    node.Arg(argAttr.Index)
                    ?? throw new KuddleSerializationException(
                        $"Expected Kdl argument at index {argAttr.Index} but found nothing"
                    );
                SetPropValue(prop, instance, targetArg);
                continue;
            }

            var propAttr = prop.GetCustomAttribute<KdlPropertyAttribute>();
            if (propAttr is not null)
            {
                var propKey = propAttr.Key ?? prop.Name.ToLowerInvariant();
                var kdlProp = node.Prop(propKey);

                if (kdlProp is null)
                    continue;

                SetPropValue(prop, instance, kdlProp);
            }

            var childAttr = prop.GetCustomAttribute<KdlNodeAttribute>();
            if (childAttr is not null)
            {
                // // Usually a collection
                var nodeName = childAttr.Name ?? prop.Name.ToLowerInvariant();
                // var groupedChildNodes = node.Children?.Nodes.GroupBy(n => n.Name) ?? [];
                // foreach (var childNode in groupedChildNodes)
                // {
                //     if (childNode.Key.Value == nodeName)
                //     {
                //         SetCollectionPropValue(prop, instance, childNode);
                //     }
                // }

                var matchingNodes = node
                    .Children?.Nodes.Where(n =>
                        n.Name.Value.Equals(nodeName, StringComparison.OrdinalIgnoreCase)
                    )
                    .ToList();

                if (matchingNodes == null || matchingNodes.Count == 0)
                    continue;

                if (prop.PropertyType.IsIEnumerable)
                {
                    SetCollectionPropValue(prop, instance, matchingNodes);
                }
                // CASE B: Scalar (string, int, etc.) -> Map Arg(0)
                else if (!prop.PropertyType.IsComplexType)
                {
                    // Expect exactly one node
                    if (matchingNodes.Count > 1)
                        throw new KuddleSerializationException(
                            $"Expected single node '{nodeName}' for scalar property, found {matchingNodes.Count}"
                        );

                    // Extract Arg 0
                    var val = matchingNodes[0].Arg(0);
                    SetPropValue(prop, instance, val!);
                }
                // CASE C: Complex Object -> Recursion
                else
                {
                    if (matchingNodes.Count > 1)
                        throw new KuddleSerializationException(
                            $"Expected single node '{nodeName}' for object property, found {matchingNodes.Count}"
                        );

                    SetSingleComplexPropValue(prop, instance, matchingNodes[0]);
                }
            }
        }
    }

    private static void SetCollectionPropValue(
        PropertyInfo collectionProp,
        object instance,
        IEnumerable<KdlNode> childNodeGroup
    )
    {
        var type = collectionProp.PropertyType;
        if (!type.IsIEnumerable)
        {
            throw new NotSupportedException(
                $"{nameof(SetCollectionPropValue)} expects a collection type, but received a {type.Name}"
            );
        }

        var elementType = GetElementType(type);
        if (!elementType.IsComplexType)
            throw new KuddleSerializationException(
                $"Collection element type '{elementType.Name}' must be a complex type"
            );

        var listType = typeof(List<>).MakeGenericType(elementType);
        var list = (IList)Activator.CreateInstance(listType)!;

        foreach (var childNode in childNodeGroup)
        {
            var element =
                Activator.CreateInstance(elementType)
                ?? throw new KuddleSerializationException(
                    $"Failed to create instance of '{elementType.Name}'"
                );

            MapToInstance(childNode, element);

            list.Add(element);
        }

        object finalValue = MapCollection(type, elementType, list);

        collectionProp.SetValue(instance, finalValue);
    }

    private static object MapCollection(Type targetType, Type elementType, IList list)
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

    static Type GetElementType(Type collectionType)
    {
        return collectionType.IsArray ? collectionType.GetElementType()!
            : collectionType.IsGenericType ? collectionType.GetGenericArguments()[0]
            : throw new KuddleSerializationException(
                $"Unsupported collection type '{collectionType.FullName}'"
            );
    }

    private static void SetPropValue(PropertyInfo prop, object instance, KdlValue kdlValue)
    {
        if (!TryConvertFromKdlValue(kdlValue, prop.PropertyType, out var result))
        {
            throw new KuddleSerializationException(
                $"Cannot convert KDL value to property {prop.Name} ({prop.PropertyType})"
            );
        }

        prop.SetValue(instance, result);
    }

    private static bool TryConvertFromKdlValue(
        KdlValue kdlValue,
        Type targetType,
        out object? result
    )
    {
        result = null;
        if (kdlValue is KdlNull)
        {
            if (!targetType.IsNullable())
                return false;
            result = null;
            return true;
        }

        Type underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (underlying == typeof(string) && kdlValue.TryGetString(out var stringVal))
        {
            result = stringVal;
            return true;
        }

        if (underlying == typeof(int) && kdlValue.TryGetInt(out var intVal))
        {
            result = intVal;
            return true;
        }

        if (underlying == typeof(double) && kdlValue.TryGetDouble(out var doubleVal))
        {
            result = doubleVal;
            return true;
        }

        if (underlying == typeof(long) && kdlValue.TryGetLong(out var longVal))
        {
            result = longVal;
            return true;
        }

        if (underlying == typeof(decimal) && kdlValue.TryGetDecimal(out var decimalVal))
        {
            result = decimalVal;
            return true;
        }

        if (underlying == typeof(bool) && kdlValue.TryGetBool(out var boolVal))
        {
            result = boolVal;
            return true;
        }

        if (underlying == typeof(Guid) && kdlValue.TryGetUuid(out var uuid))
        {
            result = uuid;
            return true;
        }
        if (underlying == typeof(DateTimeOffset) && kdlValue.TryGetDateTime(out var datetime))
        {
            result = datetime;
            return true;
        }
        return false;
    }

    public static string Serialize<T>(T original)
    {
        var doc = new KdlDocument();
        var type = typeof(T);
        if (!type.IsComplexType)
            throw new KuddleSerializationException();

        if (type.IsDictionary)
            throw new NotSupportedException();
        if (type.IsIEnumerable)
        {
            var nodes = SerializeCollection(original as IEnumerable);
            doc.Nodes.AddRange(nodes);
        }
        else
        {
            var node = SerializeNode(original);
            doc.Nodes.Add(node);
        }

        return KdlWriter.Write(doc);
    }

    private static KdlNode SerializeNode(object? instance, string? nodeName = null)
    {
        var type = instance?.GetType() ?? throw new ArgumentNullException();
        nodeName ??=
            type.GetCustomAttribute<KdlNodeAttribute>()?.Name ?? type.Name.ToLowerInvariant();

        var entries = new List<KdlEntry>();
        foreach (var argProp in type.GetKdlArgProps())
        {
            var value = argProp.Item1.GetValue(instance);

            if (!TryConvertToKdlValue(value, out var kdlValue))
                continue;

            var arg = new KdlArgument(kdlValue);
            entries.Add(arg);
        }

        var propProps = type.GetKdlPropProps();

        foreach (var (prop, kdlAttr) in propProps)
        {
            var key = kdlAttr.Key ?? prop.Name.ToLowerInvariant();
            if (!TryConvertToKdlValue(prop.GetValue(instance), out var value))
                continue;

            var kdlProp = new KdlProperty(KdlValue.From(key), value);
            entries.Add(kdlProp);
        }

        var childProps = type.GetKdlChildProps();
        var block = new KdlBlock();
        foreach (var (prop, childAttr) in childProps)
        {
            var childNodeName = childAttr.Name ?? prop.Name.ToLowerInvariant();
            if (prop.PropertyType.IsDictionary)
                throw new NotSupportedException();

            if (prop.PropertyType.IsIEnumerable)
            {
                var col = prop.GetValue(instance);
                var nodes = SerializeCollection(col as IEnumerable);
                block.Nodes.AddRange(nodes);
            }
        }
        return new KdlNode(KdlValue.From(nodeName))
        {
            Entries = entries,
            Children = block.Nodes.Count > 0 ? block : null,
        };
    }

    private static bool TryConvertToKdlValue(object? input, out KdlValue kdlValue)
    {
        kdlValue = KdlValue.Null;
        if (input is null)
            return true;

        var type = input.GetType();
        if (type.IsComplexType)
            throw new KuddleSerializationException("Kdl arguments should be simple scalar values");

        kdlValue = input switch
        {
            string s => KdlValue.From(s),
            int i => KdlValue.From(i),
            double d => KdlValue.From(d),
            long l => KdlValue.From(l),
            decimal m => KdlValue.From(m),
            bool b => KdlValue.From(b),
            Guid uuid => KdlValue.From(uuid),
            DateTimeOffset dto => KdlValue.From(dto),
            _ => null!,
        };
        return kdlValue is not null;
    }

    private static IEnumerable<KdlNode> SerializeCollection(IEnumerable? original)
    {
        var nodes = new List<KdlNode>();
        foreach (var item in original!)
        {
            var node = SerializeNode(item);
            nodes.Add(node);
        }

        return nodes;
    }
}

public record KdlSerializerOptions { }
