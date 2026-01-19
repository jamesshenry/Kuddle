using System;

namespace Kuddle.Serialization;

/// <summary>
/// Properties marked with this attribute will capture any KDL properties or child nodes
/// that do not match any other members on the class.
/// The property type must be an IDictionary<string, object> or IDictionary<string, KdlValue>.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class KdlExtensionDataAttribute : Attribute;
