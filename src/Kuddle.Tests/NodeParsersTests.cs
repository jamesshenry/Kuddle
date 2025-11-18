using Kuddle.Parser;

namespace Kuddle.Tests;

public class NodeParsersTests
{
    // Note: These tests are stubs until NodeParsers is implemented
    // They represent the node parsing rules from the KDL grammar:
    // base-node := slashdash? type? node-space* string (node-space+ slashdash? node-prop-or-arg)* (node-space+ slashdash node-children)* (node-space+ node-children)? (node-space+ slashdash node-children)* node-space*
    // node := base-node node-terminator
    // node-prop-or-arg := prop | value
    // node-children := '{' nodes final-node? '}'
    // node-terminator := single-line-comment | newline | ';' | eof
    // prop := string node-space* '=' node-space* value
    // value := type? node-space* (string | number | keyword)
    // type := '(' node-space* string node-space* ')'

    [Test]
    public async Task Type_ParsesSimpleType()
    {
        var sut = NodeParsers.Type;

        var input = "(string)";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo("string");
    }

    [Test]
    public async Task Type_ParsesTypeWithSpaces()
    {
        var sut = NodeParsers.Type;

        var input = "( int )";
        bool success = sut.TryParse(input, out var value, out var error);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo("int");
    }

    [Test]
    public async Task Value_ParsesStringValue()
    {
        var sut = KuddleGrammar.String;

        var input = "\"hello\"";
        bool success = sut.TryParse(input, out var value, out var error);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo("hello");
    }

    [Test]
    public async Task Value_ParsesNumberValue()
    {
        var sut = NodeParsers.Value;

        var input = "42";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Value_ParsesKeywordValue()
    {
        var sut = NodeParsers.Value;

        var input = "#true";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Value_ParsesTypedValue()
    {
        var sut = NodeParsers.Value;

        var input = "(string) \"hello\"";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Prop_ParsesSimpleProperty()
    {
        var sut = NodeParsers.Prop;

        var input = "key=value";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Prop_ParsesPropertyWithSpaces()
    {
        var sut = NodeParsers.Prop;

        var input = "key = value";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task NodeChildren_ParsesSimpleChildren()
    {
        var sut = NodeParsers.NodeChildren;

        var input = "{ child1 child2 }";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task NodeChildren_ParsesEmptyChildren()
    {
        var sut = NodeParsers.NodeChildren;

        var input = "{ }";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Node_ParsesSimpleNode()
    {
        var sut = NodeParsers.Node;

        var input = "node;";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Node_ParsesNodeWithArguments()
    {
        var sut = NodeParsers.Node;

        var input = "node arg1 arg2;";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Node_ParsesNodeWithProperties()
    {
        var sut = NodeParsers.Node;

        var input = "node prop=value other=123;";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Node_ParsesNodeWithChildren()
    {
        var sut = NodeParsers.Node;

        var input = "node { child1 child2 };";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Node_ParsesNodeWithNewlineTerminator()
    {
        var sut = NodeParsers.Node;

        var input = "node\n";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Node_ParsesNodeWithCommentTerminator()
    {
        var sut = NodeParsers.Node;

        var input = "node // comment";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Nodes_ParsesMultipleNodes()
    {
        var sut = NodeParsers.Nodes;

        var input = "node1; node2;";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Nodes_ParsesNodesWithWhitespace()
    {
        var sut = NodeParsers.Nodes;

        var input = "\n  node1;\n  node2;\n";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Prop_RejectsMissingValue()
    {
        var sut = NodeParsers.Prop;

        var input = "key=";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsFalse();
    }

    [Test]
    public async Task Prop_RejectsMissingEquals()
    {
        var sut = NodeParsers.Prop;

        var input = "key value";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsFalse();
    }

    [Test]
    public async Task Type_RejectsEmptyType()
    {
        var sut = NodeParsers.Type;

        var input = "()";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsFalse();
    }

    [Test]
    public async Task NodeChildren_RejectsUnclosedBlock()
    {
        var sut = NodeParsers.NodeChildren;

        var input = "{ child1 child2";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsFalse();
    }
}
