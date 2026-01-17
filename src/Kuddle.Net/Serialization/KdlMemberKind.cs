namespace Kuddle.Serialization;

internal enum KdlMemberKind
{
    Argument,
    Property,
    ChildNode,
    TypeAnnotation,
    ExtensionData, // Added for Rule 20
}
