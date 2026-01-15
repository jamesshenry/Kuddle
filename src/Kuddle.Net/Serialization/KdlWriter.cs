using System;
using System.Text;
using Kuddle.AST;
using Kuddle.Extensions;

namespace Kuddle.Serialization;

public class KdlWriter
{
    private readonly KdlWriterOptions _options;
    private readonly StringBuilder _sb = new();
    private int _depth = 0;

    public KdlWriter(KdlWriterOptions? options = null)
    {
        _options ??= options ?? KdlWriterOptions.Default;
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

        if (_options.StringStyle.HasFlag(KdlStringStyle.Preserve) && node.TerminatedBySemicolon)
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
        var style = _options.StringStyle;

        if (style.HasFlag(KdlStringStyle.Preserve))
        {
            WriteFormattedString(s.Value, s.Kind);
            return;
        }

        bool hasComplexControls = false;
        foreach (char c in s.Value)
        {
            if (char.IsControl(c) && c != '\n' && c != '\r' && c != '\t')
            {
                hasComplexControls = true;
                break;
            }
        }

        bool useBare = false;
        bool useRaw = false;
        bool useMulti = false;
        bool hasNewline = s.Value.Contains('\n') || s.Value.Contains('\r');

        if (!hasComplexControls)
        {
            if (style.HasFlag(KdlStringStyle.AllowBare) && IsValidBareIdentifier(s.Value))
            {
                useBare = true;
            }
            else
            {
                if (style.HasFlag(KdlStringStyle.AllowMultiline) && hasNewline)
                {
                    useMulti = true;
                }

                bool rawIsSafeForNewlines = !hasNewline || useMulti;

                if (rawIsSafeForNewlines)
                {
                    if (
                        style.HasFlag(KdlStringStyle.PreferRaw)
                        && (s.Value.Contains('"') || s.Value.Contains('\\'))
                    )
                    {
                        useRaw = true;
                    }
                    else if (
                        style.HasFlag(KdlStringStyle.RawPaths)
                        && (s.Value.Contains('/') || s.Value.Contains('\\'))
                    )
                    {
                        useRaw = true;
                    }
                }
            }
        }

        if (useBare)
        {
            _sb.Append(s.Value);
        }
        else if (useRaw)
        {
            WriteFormattedString(
                s.Value,
                StringKind.Raw | (useMulti ? StringKind.MultiLine : StringKind.Quoted)
            );
        }
        else if (useMulti)
        {
            WriteFormattedString(s.Value, StringKind.MultiLine);
        }
        else
        {
            _sb.Append('"').Append(EscapeString(s.Value)).Append('"');
        }
    }

    private void WriteFormattedString(string value, StringKind kind)
    {
        bool isRaw = kind.HasFlag(StringKind.Raw);
        bool isMulti = kind.HasFlag(StringKind.MultiLine);

        if (isRaw)
        {
            int hashCount = value.AsSpan().MaxConsecutive('#') + 1;
            string hashes = new('#', hashCount);
            string quotes = isMulti ? "\"\"\"" : "\"";

            _sb.Append(hashes).Append(quotes);

            if (isMulti)
            {
                _sb.Append(_options.NewLine);
            }

            _sb.Append(value);

            if (isMulti)
            {
                _sb.Append(_options.NewLine);
            }

            _sb.Append(quotes).Append(hashes);
        }
        else if (isMulti)
        {
            _sb.Append("\"\"\"");
            _sb.Append(_options.NewLine);
            _sb.Append(value);
            _sb.Append(_options.NewLine);
            _sb.Append("\"\"\"");
        }
        else if (kind.HasFlag(StringKind.Quoted) || !IsValidBareIdentifier(value))
        {
            _sb.Append('"');
            _sb.Append(EscapeString(value));
            _sb.Append('"');
        }
        else
        {
            _sb.Append(value);
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
