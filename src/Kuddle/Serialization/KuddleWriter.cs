using System;
using System.Text;
using Kuddle.AST;
using Kuddle.Extensions;

namespace Kuddle.Serialization;

public class KuddleWriter
{
    private readonly KuddleWriterOptions _options;
    private readonly StringBuilder _sb = new();
    private int _depth = 0;

    public KuddleWriter(KuddleWriterOptions? options = null)
    {
        _options ??= options ?? KuddleWriterOptions.Default;
    }

    public static string Write(KdlDocument document, KuddleWriterOptions? options = null)
    {
        var writer = new KuddleWriter(options);
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

        WriteString(node.Name);

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

        if (_options.RoundTrip && node.TerminatedBySemicolon)
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
            WriteString(prop.Key);
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
                _sb.Append(n.ToCanonicalString());
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
        StringKind kind;

        if (_options.RoundTrip)
        {
            kind = s.Kind;

            if (kind == StringKind.Bare && !IsValidBareIdentifier(s.Value))
            {
                kind = StringKind.Quoted;
            }
        }
        else
        {
            kind =
                IsValidBareIdentifier(s.Value) || s.Kind == StringKind.Bare
                    ? StringKind.Bare
                    : StringKind.Quoted;
        }

        switch (kind)
        {
            case StringKind.Bare:
                _sb.Append(s.Value);
                return;
            case StringKind.Quoted:
                _sb.Append('"');
                _sb.Append(EscapeString(s.Value));
                _sb.Append('"');
                return;
        }

        bool isRaw = s.Kind.HasFlag(StringKind.Raw);
        bool isMulti = s.Kind.HasFlag(StringKind.MultiLine);

        if (isRaw)
        {
            int hashCount = s.Value.AsSpan().MaxConsecutive('#') + 1;
            string hashes = new('#', hashCount);

            string quotes = isMulti ? new string('\"', 3) : new string('\"', 1);

            _sb.Append(hashes).Append(quotes);

            if (isMulti)
                _sb.Append('\n');

            _sb.Append(s.Value);

            if (isMulti)
                _sb.Append('\n');

            _sb.Append(quotes).Append(hashes);
        }
        else
        {
            if (isMulti)
            {
                _sb.Append(new string('\"', 3));
                _sb.Append(s.Value);
                _sb.Append(new string('\"', 3));
            }
            else
            {
                _sb.Append(new string('\"', 1));
                _sb.Append(EscapeString(s.Value));
                _sb.Append(new string('\"', 1));
            }
        }
    }

    private static string EscapeString(string input)
    {
        if (string.IsNullOrEmpty(input))
            return "";

        var sb = new StringBuilder(input.Length + 2);

        foreach (char c in input)
        {
            switch (c)
            {
                case '\\':
                    sb.Append("\\\\");
                    break;
                case '"':
                    sb.Append("\\\"");
                    break;
                case '\n':
                    sb.Append("\\n");
                    break;
                case '\r':
                    sb.Append("\\r");
                    break;
                case '\t':
                    sb.Append("\\t");
                    break;
                case '\b':
                    sb.Append("\\b");
                    break;
                case '\f':
                    sb.Append("\\f");
                    break;
                default:
                    // KDL allows most unicode, but you might want to escape control codes
                    if (char.IsControl(c))
                    {
                        sb.Append($"\\u{(int)c:X4}");
                    }
                    else
                    {
                        sb.Append(c);
                    }
                    break;
            }
        }
        return sb.ToString();
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
