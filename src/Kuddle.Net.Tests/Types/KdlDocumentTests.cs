using Kuddle.AST;
using Kuddle.Serialization;

namespace Kuddle.Tests.Types;

/// <summary>
/// Tests for KdlDocument construction, navigation, and serialization.
/// </summary>
public class KdlDocumentTests
{
    #region Construction Tests

    [Test]
    public async Task Constructor_CreatesEmptyDocument()
    {
        var sut = new KdlDocument();

        await Assert.That(sut.Nodes).IsEmpty();
    }

    [Test]
    public async Task Constructor_WithNodes_StoresNodes()
    {
        var node1 = new KdlNode(new KdlString("node1", StringKind.Bare));
        var node2 = new KdlNode(new KdlString("node2", StringKind.Bare));

        var sut = new KdlDocument { Nodes = [node1, node2] };

        await Assert.That(sut.Nodes).Count().IsEqualTo(2);
        await Assert.That(sut.Nodes[0].Name.Value).IsEqualTo("node1");
        await Assert.That(sut.Nodes[1].Name.Value).IsEqualTo("node2");
    }

    #endregion

    #region ToString Tests

    [Test]
    public async Task ToString_EmptyDocument_ReturnsEmptyString()
    {
        var sut = new KdlDocument();

        var result = sut.ToString();

        await Assert.That(result).IsEqualTo("");
    }

    [Test]
    public async Task ToString_WithNodes_ReturnsKdlString()
    {
        var node = new KdlNode(new KdlString("test", StringKind.Bare))
        {
            Entries = [new KdlArgument(new KdlNumber("42"))],
        };
        var sut = new KdlDocument { Nodes = [node] };

        var result = sut.ToString();

        await Assert.That(result.Trim()).IsEqualTo("test 42");
    }

    [Test]
    public async Task ToString_WithOptions_UsesOptions()
    {
        var node = new KdlNode(new KdlString("test", StringKind.Bare))
        {
            Entries = [new KdlArgument(new KdlString("value", StringKind.Quoted))],
        };
        var sut = new KdlDocument { Nodes = [node] };

        var result = sut.ToString(new KdlWriterOptions { StringStyle = KdlStringStyle.Preserve });

        await Assert.That(result.Trim()).IsEqualTo("test \"value\"");
    }

    #endregion

    #region Record Equality Tests

    [Test]
    public async Task Equality_SameListInstance_AreEqual()
    {
        var node = new KdlNode(new KdlString("test", StringKind.Bare));
        var nodes = new List<KdlNode> { node };
        var doc1 = new KdlDocument { Nodes = nodes };
        var doc2 = new KdlDocument { Nodes = nodes };

        await Assert.That(doc1).IsEqualTo(doc2);
    }

    [Test]
    public async Task Equality_DifferentListInstances_AreNotEqual()
    {
        // Records with List<T> use reference equality for the list
        var node = new KdlNode(new KdlString("test", StringKind.Bare));
        var doc1 = new KdlDocument { Nodes = [node] };
        var doc2 = new KdlDocument { Nodes = [node] };

        await Assert.That(doc1).IsNotEqualTo(doc2);
    }

    [Test]
    public async Task Equality_DifferentNodes_AreNotEqual()
    {
        var doc1 = new KdlDocument
        {
            Nodes = [new KdlNode(new KdlString("node1", StringKind.Bare))],
        };
        var doc2 = new KdlDocument
        {
            Nodes = [new KdlNode(new KdlString("node2", StringKind.Bare))],
        };

        await Assert.That(doc1).IsNotEqualTo(doc2);
    }

    #endregion

    #region Parsing Integration Tests

    [Test]
    public async Task Parse_SimpleDocument_CreatesCorrectStructure()
    {
        var kdl = """
            node1 "arg1" key="value"
            node2 123
            """;

        var sut = KdlReader.Read(kdl);

        await Assert.That(sut.Nodes).Count().IsEqualTo(2);
        await Assert.That(sut.Nodes[0].Name.Value).IsEqualTo("node1");
        await Assert.That(sut.Nodes[1].Name.Value).IsEqualTo("node2");
    }

    [Test]
    public async Task Parse_NestedDocument_CreatesCorrectStructure()
    {
        var kdl = """
            parent {
                child "value"
            }
            """;

        var sut = KdlReader.Read(kdl);

        await Assert.That(sut.Nodes).Count().IsEqualTo(1);
        await Assert.That(sut.Nodes[0].HasChildren).IsTrue();
        await Assert.That(sut.Nodes[0].Children!.Nodes).Count().IsEqualTo(1);
    }

    #endregion
}
