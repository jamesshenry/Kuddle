using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Kuddle.Serialization;

internal static class TypeExtensions
{
    extension(Type type)
    {
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

        internal bool IsKdlScalar =>
            type.IsPrimitive
            || type.IsEnum
            || type == typeof(string)
            || type == typeof(decimal)
            || type == typeof(DateTime)
            || type == typeof(DateTimeOffset)
            || type == typeof(Guid)
            || type == typeof(TimeSpan)
            || type == typeof(DateOnly)
            || type == typeof(TimeOnly);

        public Type? GetCollectionElementType()
        {
            if (type == typeof(string))
                return null;
            // Array
            if (type.IsArray)
                return type.GetElementType();

            // IEnumerable<T>
            var enumerable = type.GetInterfaces()
                .Append(type)
                .FirstOrDefault(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                );

            if (enumerable != null)
                return enumerable.GetGenericArguments()[0];

            // Not a collection
            return null;
        }
    }
}
