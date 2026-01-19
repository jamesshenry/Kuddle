using System.Collections.Generic;
using System.Threading;

namespace Kuddle.Serialization;

/// <summary>
/// Serializes and deserializes C# objects to/from KDL format.
/// </summary>
public static class KdlSerializer
{
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
        options ??= KdlSerializerOptions.Default;

        var doc = KdlReader.Read(text, options.Reader);

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
        options ??= KdlSerializerOptions.Default;

        var doc = KdlReader.Read(text, options.Reader);

        return ObjectDeserializer.DeserializeDocument<T>(doc, options);
    }

    /// <summary>
    /// Serializes an object to a KDL string.
    /// </summary>
    public static string Serialize<T>(T instance, KdlSerializerOptions? options = null)
    {
        options ??= KdlSerializerOptions.Default;

        var doc = ObjectSerializer.SerializeDocument(instance, options);

        return KdlWriter.Write(doc, options.Writer);
    }
}
