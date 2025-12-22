using System;
using System.Reflection;

namespace Kuddle.Serialization;

/// <summary>
/// Represents a mapping from a C# property to a KDL entry (argument, property, or child node).
/// </summary>
internal sealed record KdlMemberInfo(PropertyInfo Property, Attribute? Attribute)
{
    public bool IsArgument => Attribute is KdlArgumentAttribute;
    public bool IsProperty => Attribute is KdlPropertyAttribute;
    public bool IsNode => Attribute is KdlNodeAttribute;
    public bool IsWrappedCollection => Attribute is KdlNodeCollectionAttribute;
    public string? CollectionElementName => (Attribute as KdlNodeCollectionAttribute)?.ElementName;
    public bool IsNodeDictionary => Attribute is KdlNodeDictionaryAttribute;

    //TODO: Add support for property dictionaries
    public bool IsPropertyDictionary => false;

    //TODO: Add support for keyed node collections
    public bool IsKeyedNodeCollection => false;

    public int ArgumentIndex => Attribute is KdlArgumentAttribute arg ? arg.Index : -1;

    public string Name =>
        Attribute switch
        {
            KdlPropertyAttribute p => p.Key ?? Property.Name.ToLowerInvariant(),
            KdlNodeAttribute n => n.Name ?? Property.Name.ToLowerInvariant(),
            KdlNodeDictionaryAttribute nd => nd.Name ?? Property.Name.ToLowerInvariant(),
            KdlNodeCollectionAttribute nc => nc.NodeName ?? Property.Name.ToLowerInvariant(),
            _ => Property.Name.ToLowerInvariant(),
        };

    public string? TypeAnnotation => null;
}
