using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Kuddle.Serialization;

internal static class TypeExtensions
{
    extension(Type type)
    {
        internal bool IsNodeDefinition =>
            type.GetProperties()
                .Any(p =>
                    p.GetCustomAttribute<KdlArgumentAttribute>() != null
                    || p.GetCustomAttribute<KdlPropertyAttribute>() != null
                );
        internal bool IsComplexType =>
            !type.IsValueType
            && !type.IsPrimitive
            && type != typeof(string)
            && type != typeof(object)
            && !type.IsInterface
            && !type.IsAbstract;

        internal bool IsDictionary =>
            type.IsGenericType
            && type.GetInterfaces()
                .Any(i =>
                    i.IsGenericType
                    && (
                        i.GetGenericTypeDefinition() == typeof(IDictionary<,>)
                        || i.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>)
                    )
                );

        internal bool IsIEnumerable =>
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

        internal (PropertyInfo, KdlNodeAttribute)[] GetKdlChildProps() =>
            type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => new
                {
                    Property = p,
                    ChildAttr = p.GetCustomAttribute<KdlNodeAttribute>(),
                })
                .Where(x => x.ChildAttr is not null)
                .Select(x => (x.Property, x.ChildAttr!))
                .ToArray();

        public bool IsNullable() => Nullable.GetUnderlyingType(type) != null;
    }
}
