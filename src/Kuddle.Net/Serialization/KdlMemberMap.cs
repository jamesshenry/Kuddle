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
        string? typeAnnotation = null,
        bool isFlattened = false
    )
    {
        Property = property;
        Kind = kind;
        KdlName = kdlName;
        ArgumentIndex = argumentIndex;
        TypeAnnotation = typeAnnotation;
        IsFlattened = isFlattened;
        IsDictionary = property.PropertyType.IsDictionary;
        var elementType = property.PropertyType.GetCollectionElementType();
        IsCollection = elementType != null;
        if (IsDictionary && elementType != null)
        {
            DictionaryKeyProperty = elementType.GetProperty("Key");
            DictionaryValueProperty = elementType.GetProperty("Value");
        }
        ElementType = elementType;
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
    public bool IsFlattened { get; }

    public object? GetValue(object instance) => Property.GetValue(instance);

    public void SetValue(object instance, object? value) => Property.SetValue(instance, value);
}
