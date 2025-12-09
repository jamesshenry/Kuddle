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

        foreach (var entry in node.Entries)
        {
            _sb.Append(_options.SpaceAfterProp);
            WriteEntry(entry);
        }

        if (node.Children?.Nodes.Count > 0)
        {
            _sb.Append(" {");
            _sb.Append(_options.NewLine);

            _depth++;
            foreach (var child in node.Children.Nodes)
            {
                WriteIndent();
                WriteNode(child);
                _sb.Append(_options.NewLine);
            }
            _depth--;

            WriteIndent();
            _sb.Append('}');
        }
        else if (node.TerminatedBySemicolon)
        {
            _sb.Append(';');
        }
    }

    private void WriteEntry(KdlEntry entry)
    {
        if (entry is KdlArgument arg)
        {
            WriteValue(arg.Value);
        }
        else if (entry is KdlProperty prop)
        {
            WriteIdentifier(prop.Key.Value);
            _sb.Append('=');
            WriteValue(prop.Value);
        }
    }

    private void WriteValue(KdlValue value)
    {
        if (value.TypeAnnotation != null)
        {
            _sb.Append('(');
            WriteIdentifier(value.TypeAnnotation);
            _sb.Append(')');
        }

        switch (value)
        {
            case KdlNumber n:
                _sb.Append(n.RawValue);
                break;
            case KdlBool b:
                _sb.Append(b.Value ? "#true" : "#false");
                break;
            case KdlNull:
                _sb.Append("#null");
                break;
            case KdlString s:
                WriteString(s);
                break;
        }
    }

    private void WriteIdentifier(string value)
    {
        if (IsValidBareIdentifier(value))
        {
            _sb.Append(value);
        }
        else
        {
            WriteQuotedString(value);
        }
    }

    private void WriteString(KdlString s)
    {
        if (s.Kind == StringKind.Raw)
        {
            // TODO: Handle raw strings
        }
        else if (s.Kind == StringKind.MultiLine)
        { // TODO: Robust multiline logic is complex (dedenting).
            // Fallback to safe quoted string for now to ensure validity.
            WriteQuotedString(s.Value);
        }
        else
        {
            WriteQuotedString(s.Value);
        }
    }

    private void WriteQuotedString(string val)
    {
        _sb.Append('"');
        foreach (char c in val)
        {
            switch (c)
            {
                case '\\':
                    _sb.Append("\\\\");
                    break;
                case '"':
                    _sb.Append("\\\"");
                    break;
                case '\b':
                    _sb.Append("\\b");
                    break;
                case '\f':
                    _sb.Append("\\f");
                    break;
                case '\n':
                    _sb.Append("\\n");
                    break;
                case '\r':
                    _sb.Append("\\r");
                    break;
                case '\t':
                    _sb.Append("\\t");
                    break;
                default:
                    if (char.IsControl(c) || (_options.EscapeUnicode && c > 127))
                    {
                        _sb.Append($"\\u{(int)c:X4}");
                    }
                    else
                    {
                        _sb.Append(c);
                    }
                    break;
            }
        }
        _sb.Append('"');
    }

    private void WriteIndent()
    {
        for (int i = 0; i < _depth; i++)
            _sb.Append(_options.IndentChar);
    }

    private static bool IsValidBareIdentifier(string id)
    {
        if (string.IsNullOrEmpty(id))
            return false;
        if (id == "true" || id == "false" || id == "null")
            return false;
        if (char.IsDigit(id[0]))
            return false;

        foreach (char c in id)
        {
            if (char.IsWhiteSpace(c) || "()[]{}/\\\"#;=".Contains(c))
                return false;
        }
        return true;
    }
}

public record KdlWriterOptions
{
    public static KdlWriterOptions Default => new();

    public string IndentChar { get; init; } = "    ";
    public string NewLine { get; init; } = "\n";
    public string SpaceAfterProp { get; init; } = " ";
    public bool EscapeUnicode { get; init; } = false;
}
