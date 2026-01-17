using Kuddle.AST;
using Kuddle.Parser;

namespace Kuddle.Tests.Parsing;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

public class NodeParsersTests
{
    [Test]
    public async Task Type_ParsesSimpleType()
    {
        var sut = KdlGrammar.Type;
        var input = "(string)";

        bool success = sut.TryParse(input, out var result);

        await Assert.That(success).IsTrue();
        await Assert.That(result.Value).IsEqualTo("string");
    }

    [Test]
    public async Task Prop_ParsesSimpleProperty()
    {
        var sut = KdlGrammar.Node;
        var input = "node key=value";

        bool success = sut.TryParse(input, out var node);

        await Assert.That(success).IsTrue();
        await Assert.That(node.Entries).Count().IsEqualTo(1);

        var prop = node.Entries[0] as KdlProperty;
        await Assert.That(prop).IsNotNull();
        await Assert.That(prop!.Key.Value).IsEqualTo("key");
        await Assert.That(((KdlString)prop.Value).Value).IsEqualTo("value");
    }

    [Test]
    public async Task Node_ParsesComplexLine()
    {
        var sut = KdlGrammar.Node;
        var input = "(my-type)node 123 key=\"value\";";

        bool success = sut.TryParse(input, out var node);

        await Assert.That(success).IsTrue();

        await Assert.That(node.Name.Value).IsEqualTo("node");
        await Assert.That(node.TypeAnnotation).IsEqualTo("my-type");
        await Assert.That(node.TerminatedBySemicolon).IsTrue();

        await Assert.That(node.Entries).Count().IsEqualTo(2);

        var arg = node.Entries[0] as KdlArgument;
        await Assert.That(arg).IsNotNull();
        await Assert.That(((KdlNumber)arg!.Value).ToInt32()).IsEqualTo(123);

        var prop = node.Entries[1] as KdlProperty;
        await Assert.That(prop).IsNotNull();
        await Assert.That(prop!.Key.Value).IsEqualTo("key");
    }

    [Test]
    public async Task Node_ParsesChildren()
    {
        var sut = KdlGrammar.Node;
        var input = "parent { child; }";

        bool success = sut.TryParse(input, out var node, out var error);

        await Assert.That(success).IsTrue();
        await Assert.That(node.Name.Value).IsEqualTo("parent");
        await Assert.That(node.Children).IsNotNull();
        await Assert.That(node.Children!.Nodes).Count().IsEqualTo(1);
        await Assert.That(node.Children.Nodes[0].Name.Value).IsEqualTo("child");
    }

    [Test]
    public async Task Node_ParsesMixedContent()
    {
        var sut = KdlGrammar.Node;
        var input = "(type)node 10 prop=#true { child; }";

        bool success = sut.TryParse(input, out var node);

        await Assert.That(success).IsTrue();

        // Metadata
        await Assert.That(node.Name.Value).IsEqualTo("node");
        await Assert.That(node.TypeAnnotation).IsEqualTo("type");

        // Entries
        await Assert.That(node.Entries).Count().IsEqualTo(2);
        await Assert
            .That(((KdlNumber)((KdlArgument)node.Entries[0]).Value).ToInt32())
            .IsEqualTo(10);
        await Assert.That(((KdlBool)((KdlProperty)node.Entries[1]).Value).Value).IsTrue();

        // Children
        await Assert.That(node.Children).IsNotNull();
        await Assert.That(node.Children!.Nodes).Count().IsEqualTo(1);
    }

    [Test]
    public async Task Node_SlashDash_SkipsNode()
    {
        // This tests the logic in 'Nodes' (plural) parser usually
        var sut = KdlGrammar.Document;
        var input = "node1; /- node2; node3;";

        bool success = sut.TryParse(input, out var doc);

        await Assert.That(success).IsTrue();
        await Assert.That(doc.Nodes).Count().IsEqualTo(2);
        await Assert.That(doc.Nodes[0].Name.Value).IsEqualTo("node1");
        await Assert.That(doc.Nodes[1].Name.Value).IsEqualTo("node3");
    }

    [Test]
    public async Task Node_SlashDash_SkipsArg()
    {
        var sut = KdlGrammar.Node;
        var input = "node 1 /- 2 3";

        bool success = sut.TryParse(input, out var node);

        await Assert.That(success).IsTrue();
        await Assert.That(node.Entries).Count().IsEqualTo(2);

        // Entry 0 should be 1
        var arg1 = node.Entries[0] as KdlArgument;
        await Assert.That(((KdlNumber)arg1!.Value).ToInt32()).IsEqualTo(1);

        // Entry 1 should be 3 (2 was skipped)
        var arg2 = node.Entries[1] as KdlArgument;
        await Assert.That(((KdlNumber)arg2!.Value).ToInt32()).IsEqualTo(3);
    }

    [Test]
    public async Task SlashDash_SkipsProperty()
    {
        var sut = KdlGrammar.Node;
        var input = "node key=1 /- skipped=2 valid=3";

        bool success = sut.TryParse(input, out var node);

        await Assert.That(success).IsTrue();
        await Assert.That(node.Entries).Count().IsEqualTo(2);

        var p1 = node.Entries[0] as KdlProperty;
        await Assert.That(p1!.Key.Value).IsEqualTo("key");

        var p2 = node.Entries[1] as KdlProperty;
        await Assert.That(p2!.Key.Value).IsEqualTo("valid");
    }

    [Test]
    public async Task Node_SlashDash_SkipsChildrenBlock()
    {
        var sut = KdlGrammar.Node;
        // Parsing a node that has a slash-dashed children block
        var input = "node /- { child; }";

        bool success = sut.TryParse(input, out var node);

        await Assert.That(success).IsTrue();
        await Assert.That(node.Name.Value).IsEqualTo("node");
        // The children block should be null because it was skipped
        await Assert.That(node.Children).IsNull();
    }

    [Test]
    public async Task Nodes_ParsesNodesWithWhitespace()
    {
        var sut = KdlGrammar.Nodes;
        var input =
            @"
            node1;
            
            node2;
        ";

        bool success = sut.TryParse(input, out var nodes);

        await Assert.That(success).IsTrue();
        await Assert.That(nodes).Count().IsEqualTo(2);
    }
}
#pragma warning restore CS8602 // Dereference of a possibly null reference.
