using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Kuddle.AST;
using Kuddle.Extensions;

namespace Kuddle.Serialization;

public static class KdlSerializer
{
    public static async Task<T> Deserialize<T>(string text, KdlSerializerOptions? options = null)
    {
        var t = typeof(T);
        KdlDocument document;
        if (t.IsDictionary)
        {
            throw new KuddleSerializationException(
                "Deserialization to dictionaries is not supported"
            );
        }
        else if (t.IsIEnumerable)
        {
            var genericTypeDef = t.GetGenericTypeDefinition();
            var args = t.GetGenericArguments();
            var listType = typeof(List<>).MakeGenericType(args);
            var listInstance = (IList)Activator.CreateInstance(listType)!;

            document = await KuddleReader.ReadAsync(text);

            var firstName = document.Nodes.FirstOrDefault()?.Name;

            foreach (var node in document.Nodes)
            {
                if (node.Name != firstName)
                {
                    throw new KuddleSerializationException(
                        "All root nodes must have the same name."
                    );
                }
            }

            return (T)listInstance;
        }

        if (!t.IsComplexType)
        {
            throw new KuddleSerializationException(
                $"Cannot deserialize type '{t.FullName}' as a complex object. "
                    + "Ensure the type is a concrete class with a public parameterless constructor."
            );
        }
        var instance = Activator.CreateInstance<T>();
        document = await KuddleReader.ReadAsync(text);

        if (document.Nodes.Count != 1)
        {
            throw new KuddleSerializationException(
                "Kdl document must have a single root node to map to an instance T"
            );
        }
        if (
            !typeof(T).Name.Equals(
                document.Nodes[0].Name.Value,
                StringComparison.InvariantCultureIgnoreCase
            )
        )
        {
            throw new KuddleSerializationException(
                $"Node name '{document.Nodes[0].Name}' does not match target type name '{typeof(T).Name}'."
            );
        }
        MapToInstance(document.Nodes[0], instance);

        return instance;
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

            var childAttr = prop.GetCustomAttribute<KdlChildrenAttribute>();
            if (childAttr is not null)
            {
                var nodeName = childAttr.ChildNodeName ?? prop.Name.ToLowerInvariant();
                var groupedChildNodes = node.Children?.Nodes.GroupBy(n => n.Name) ?? [];
                foreach (var childNode in groupedChildNodes)
                {
                    if (childNode.Key.Value == nodeName)
                    {
                        SetCollectionPropValue(prop, instance, childNode);
                    }
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

        // IEnumerable<T>, IReadOnlyList<T>, etc.
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

        return KuddleWriter.Write(doc);
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
            var childNodeName = childAttr.ChildNodeName ?? prop.Name.ToLowerInvariant();
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

internal static class TypeExtensions
{
    extension(Type type)
    {
        public bool IsComplexType =>
            !type.IsValueType
            && !type.IsPrimitive
            && type != typeof(string)
            && type != typeof(object)
            && !type.IsInterface
            && !type.IsAbstract;

        public bool IsDictionary =>
            type.IsGenericType
            && type.GetInterfaces()
                .Any(i =>
                    i.IsGenericType
                    && (
                        i.GetGenericTypeDefinition() == typeof(IDictionary<,>)
                        || i.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>)
                    )
                );

        public bool IsIEnumerable =>
            type != typeof(string)
            && !type.IsDictionary
            && type.IsAssignableTo(typeof(IEnumerable));

        internal (PropertyInfo, KdlArgumentAttribute)[] GetKdlArgProps() =>
            type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => new
                {
                    Property = p,
                    ArgAttr = p.GetCustomAttribute<KdlArgumentAttribute>(),
                })
                .Where(x => x.ArgAttr is not null)
                .OrderBy(x => x.ArgAttr!.Index)
                .Select(x => (x.Property, x.ArgAttr!))
                .ToArray();

        internal (PropertyInfo, KdlPropertyAttribute)[] GetKdlPropProps() =>
            type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => new
                {
                    Property = p,
                    PropAttr = p.GetCustomAttribute<KdlPropertyAttribute>(),
                })
                .Where(x => x.PropAttr is not null)
                .Select(x => (x.Property, x.PropAttr!))
                .ToArray();

        internal (PropertyInfo, KdlChildrenAttribute)[] GetKdlChildProps() =>
            type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => new
                {
                    Property = p,
                    ChildAttr = p.GetCustomAttribute<KdlChildrenAttribute>(),
                })
                .Where(x => x.ChildAttr is not null)
                .Select(x => (x.Property, x.ChildAttr!))
                .ToArray();

        public bool IsNullable() => Nullable.GetUnderlyingType(type) != null;
    }
}
