using Kuddle.AST;
using Kuddle.Serialization;
using Microsoft.Extensions.Configuration;

namespace Kuddle.Extensions.Configuration;

internal sealed class KdlConfigurationFileParser
{
    private readonly Dictionary<string, string?> _data = new(StringComparer.OrdinalIgnoreCase);
    private readonly Stack<string> _paths = [];

    private KdlConfigurationFileParser() { }

    internal static IDictionary<string, string?> Parse(
        Stream stream,
        KdlSerializerOptions? serializerOptions = null
    )
    {
        using var reader = new StreamReader(stream);
        var text = reader.ReadToEnd();

        var doc = KdlReader.Read(text);

        var parser = new KdlConfigurationFileParser();
        parser.VisitNodes(doc.Nodes);

        return parser._data;
    }

    private void VisitNodes(IEnumerable<KdlNode> nodes)
    {
        var groups = nodes.GroupBy(n => n.Name.Value, StringComparer.OrdinalIgnoreCase);

        foreach (var group in groups)
        {
            var nodesInGroup = group.ToList();

            // It's an array if there's more than one, or if the node is named "-" (KDL list convention)
            var isArray = nodesInGroup.Count > 1 || group.Key == "-";

            for (int i = 0; i < nodesInGroup.Count; i++)
            {
                var node = nodesInGroup[i];
                bool namePushed = false;
                bool indexPushed = false;

                // 1. Push the Node Name (unless it's the anonymous list marker "-")
                if (group.Key != "-")
                {
                    _paths.Push(group.Key);
                    namePushed = true;
                }

                // 2. Push the Index if this is an array element
                if (isArray)
                {
                    _paths.Push(i.ToString());
                    indexPushed = true;
                }

                ProcessNode(node);

                // Pop what we pushed to clean up the stack for the next sibling
                if (indexPushed)
                    _paths.Pop();
                if (namePushed)
                    _paths.Pop();
            }
        }
        void ProcessNode(KdlNode node)
        {
            var args = node.Arguments.ToList();
            var props = node.Properties.ToList();
            bool hasChildren = node.HasChildren;

            // SCENARIO A: Simple Value
            // If the node has exactly 1 argument and NO properties/children, it's a leaf value.
            // e.g., title "My App" maps to key "title" with value "My App"
            if (args.Count == 1 && props.Count == 0 && !hasChildren)
            {
                var key = ConfigurationPath.Combine(_paths.Reverse());
                _data[key] = ValueToString(args[0]);
            }
            else
            {
                // SCENARIO B: Complex Object
                // Treat arguments as indexed children (e.g., endpoints:0, endpoints:1)
                for (int i = 0; i < args.Count; i++)
                {
                    var key = ConfigurationPath.Combine(_paths.Reverse().Append(i.ToString()));
                    _data[key] = ValueToString(args[i]);
                }

                // Treat properties as named children
                foreach (var prop in props)
                {
                    var key = ConfigurationPath.Combine(_paths.Reverse().Append(prop.Key.Value));
                    _data[key] = ValueToString(prop.Value);
                }

                // Recurse into children
                if (hasChildren)
                {
                    VisitNodes(node.Children!.Nodes);
                }
            }
        }
    }

    private static string? ValueToString(KdlValue value)
    {
        return value switch
        {
            KdlNull => null,
            KdlBool b => b.Value ? "true" : "false",
            KdlNumber n => n.ToCanonicalString(),
            KdlString s => s.Value,
            _ => value.ToString(),
        };
    }
}
