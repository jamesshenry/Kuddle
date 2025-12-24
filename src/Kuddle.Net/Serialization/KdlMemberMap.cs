using System;
using System.Reflection;

namespace Kuddle.Serialization;

internal sealed record KdlMemberMap
{
    public KdlMemberMap(
        PropertyInfo property,
        KdlMemberKind kind,
        string kdlName,
        int argumentIndex = -1,
        string? typeAnnotation = null
    )
    {
        Property = property;
        Kind = kind;
        KdlName = kdlName;
        ArgumentIndex = argumentIndex;
        TypeAnnotation = typeAnnotation;
        IsDictionary = property.PropertyType.IsDictionary;
        ElementType = property.PropertyType.GetCollectionElementType();
        IsCollection = ElementType != null;
        if (IsDictionary && ElementType != null)
        {
            DictionaryKeyProperty = ElementType.GetProperty("Key");
            DictionaryValueProperty = ElementType.GetProperty("Value");
        }
    }

    public PropertyInfo Property { get; }
    public KdlMemberKind Kind { get; }
    public string KdlName { get; }
    public int ArgumentIndex { get; }
    public Type? ElementType { get; }
    public bool IsCollection { get; }
    public bool IsDictionary { get; }
    public string? TypeAnnotation { get; }

    public PropertyInfo? DictionaryKeyProperty { get; }
    public PropertyInfo? DictionaryValueProperty { get; }

    public object? GetValue(object instance) => Property.GetValue(instance);

    public void SetValue(object instance, object? value) => Property.SetValue(instance, value);
}
