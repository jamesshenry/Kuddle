using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using Kuddle.AST;
using Kuddle.Extensions;

namespace Kuddle.Serialization;

/// <summary>
/// Serializes and deserializes C# objects to/from KDL format.
/// </summary>
public static class KdlSerializer
{
    #region Deserialization

    /// <summary>
    /// Deserializes a KDL document containing multiple nodes of type T.
    /// </summary>
    public static IEnumerable<T> DeserializeMany<T>(
        string text,
        KdlSerializerOptions? options = null,
        CancellationToken cancellationToken = default
    )
        where T : new()
    {
        var doc = KdlReader.Read(text);
        var metadata = KdlTypeInfo.For<T>();

        foreach (var node in doc.Nodes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return ObjectDeserializer.DeserializeNode<T>(node, options);
        }
    }

    /// <summary>
    /// Deserializes a KDL document to a single object of type T.
    /// </summary>
    public static T Deserialize<T>(string text, KdlSerializerOptions? options = null)
        where T : new()
    {
        var doc = KdlReader.Read(text);

        return ObjectDeserializer.DeserializeDocument<T>(doc, options);
    }

    #endregion

    #region Serialization

    /// <summary>
    /// Serializes an object to a KDL string.
    /// </summary>
    public static string Serialize<T>(T instance, KdlSerializerOptions? options = null)
    {
        var doc = ObjectSerializer.SerializeDocument(instance, options);
        return KdlWriter.Write(doc);
    }

    /// <summary>
    /// Serializes multiple objects to a KDL string.
    /// </summary>
    public static string SerializeMany<T>(
        IEnumerable<T> items,
        KdlSerializerOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(items);

        var doc = new KdlDocument();

        foreach (var item in items)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (item is null)
                continue;

            var node = ObjectSerializer.SerializeNode(item, options);
            doc.Nodes.Add(node);
        }

        return KdlWriter.Write(doc);
    }

    #endregion

    #region Helpers


    #endregion
}
