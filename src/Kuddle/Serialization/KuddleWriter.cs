using System;
using System.Text;
using Kuddle.AST;

namespace Kuddle.Serialization;

public class KdlWriter
{
    private readonly KdlWriterOptions _options;
    private readonly StringBuilder _sb = new();
    private int _depth = 0;

    public KdlWriter(KdlWriterOptions? options = null)
    {
        _options ??= KdlWriterOptions.Default;
    }

    public static string Write(KdlDocument document, KdlWriterOptions? options = null)
    {
        var writer = new KdlWriter(options);
        writer.WriteDocument(document);
        return writer._sb.ToString();
    }

    private void WriteDocument(KdlDocument document)
    {
        foreach (var node in document.Nodes)
        {
            WriteNode(node);
            _sb.Append(_options.NewLine);
        }
    }

    private void WriteNode(KdlNode node)
    {
        if (node.TypeAnnotation != null)
        {
            _sb.Append('(');
            WriteIdentifier(node.TypeAnnotation);
            _sb.Append(')');
        }

        WriteIdentifier(node.Name.Value);
    }

    private void WriteIdentifier(string typeAnnotation)
    {
        throw new NotImplementedException();
    }
}

public record KdlWriterOptions
{
    public static KdlWriterOptions Default => new();

    public string NewLine => "\n";
}
