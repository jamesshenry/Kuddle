using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Kuddle.AST;

namespace Kuddle.Serialization;

public static class KdlSerializer
{
    public static async Task<T> Deserialize<T>(string text, KdlSerializerOptions? options = null)
        where T : new()
    {
        var instance = Activator.CreateInstance<T>();
        KdlDocument document = await KuddleReader.ReadAsync(text);

        MapToInstance(document.Nodes, instance);

        return instance;
    }

    private static void MapToInstance<T>(IEnumerable<KdlNode> nodes, T instance)
        where T : new()
    {
        DisplayGenericType(typeof(T));
        var props = instance!
            .GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite && p.GetCustomAttribute<KdlIgnoreAttribute>() == null);

        foreach (var prop in props)
        {
            var nodeAttr = prop.GetCustomAttribute<KdlNodeAttribute>();
            if (nodeAttr is null || !prop.PropertyType.IsComplexType())
                return;

            var name = nodeAttr.Name ?? prop.Name.ToLowerInvariant();

            var matchingNodes = nodes.Where(n => n.Name.Value == name).ToList();

            if (matchingNodes.Count == 0)
                return;
        }
    }

    // The following method displays information about a generic
    // type.
    private static void DisplayGenericType(Type t)
    {
        Console.WriteLine($"\r\n {t}");
        Console.WriteLine($"   Is this a generic type? {t.IsGenericType}");
        Console.WriteLine($"   Is this a generic type definition? {t.IsGenericTypeDefinition}");

        // Get the generic type parameters or type arguments.
        Type[] typeParameters = t.GetGenericArguments();

        Console.WriteLine($"   List {typeParameters.Length} type arguments:");
        foreach (Type tParam in typeParameters)
        {
            if (tParam.IsGenericParameter)
            {
                DisplayGenericParameter(tParam);
            }
            else
            {
                Console.WriteLine($"      Type argument: {tParam}");
            }
        }
    }

    // Displays information about a generic type parameter.
    private static void DisplayGenericParameter(Type tp)
    {
        Console.WriteLine(
            $"      Type parameter: {tp.Name} position {tp.GenericParameterPosition}"
        );

        foreach (Type iConstraint in tp.GetGenericParameterConstraints())
        {
            if (iConstraint.IsInterface)
            {
                Console.WriteLine($"         Interface constraint: {iConstraint}");
            }
        }

        Console.WriteLine($"         Base type constraint: {tp.BaseType ?? tp.BaseType: None}");

        GenericParameterAttributes sConstraints =
            tp.GenericParameterAttributes & GenericParameterAttributes.SpecialConstraintMask;

        if (sConstraints == GenericParameterAttributes.None)
        {
            Console.WriteLine("         No special constraints.");
        }
        else
        {
            if (
                GenericParameterAttributes.None
                != (sConstraints & GenericParameterAttributes.DefaultConstructorConstraint)
            )
            {
                Console.WriteLine("         Must have a parameterless constructor.");
            }
            if (
                GenericParameterAttributes.None
                != (sConstraints & GenericParameterAttributes.ReferenceTypeConstraint)
            )
            {
                Console.WriteLine("         Must be a reference type.");
            }
            if (
                GenericParameterAttributes.None
                != (sConstraints & GenericParameterAttributes.NotNullableValueTypeConstraint)
            )
            {
                Console.WriteLine("         Must be a non-nullable value type.");
            }
        }
    }
}

public record KdlSerializerOptions { }

public static class PropertyExtensions
{
    extension(Type type)
    {
        public bool IsComplexType()
        {
            return !type.IsValueType && !type.IsPrimitive && type != typeof(string);
        }
    }
}
