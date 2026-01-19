using Kuddle.AST;
using Kuddle.Serialization;

namespace Kuddle.Tests.Types;

/// <summary>
/// Tests for KdlNode construction, indexers, and property access.
/// </summary>
public class KdlNodeTests
{
    #region Construction Tests

    [Test]
    public async Task Constructor_WithName_SetsName()
    {
        var name = new KdlString("myNode", StringKind.Bare);

        var sut = new KdlNode(name);

        await Assert.That(sut.Name.Value).IsEqualTo("myNode");
        await Assert.That(sut.Entries).IsEmpty();
        await Assert.That(sut.Children).IsNull();
    }

    [Test]
    public async Task Constructor_WithEntries_SetsEntries()
    {
        var name = new KdlString("node", StringKind.Bare);
        var arg = new KdlArgument(new KdlNumber("42"));
        var prop = new KdlProperty(
            new KdlString("key", StringKind.Bare),
            new KdlString("value", StringKind.Quoted)
        );

        var sut = new KdlNode(name) { Entries = [arg, prop] };

        await Assert.That(sut.Entries).Count().IsEqualTo(2);
    }

    [Test]
    public async Task Constructor_WithChildren_SetsChildren()
    {
        var name = new KdlString("parent", StringKind.Bare);
        var childNode = new KdlNode(new KdlString("child", StringKind.Bare));
        var block = new KdlBlock { Nodes = [childNode] };

        var sut = new KdlNode(name) { Children = block };

        await Assert.That(sut.Children).IsNotNull();
        await Assert.That(sut.Children!.Nodes).Count().IsEqualTo(1);
    }

    #endregion

    #region Indexer Tests

    [Test]
    public async Task Indexer_ExistingProperty_ReturnsValue()
    {
        var kdl = "node key=\"value\"";
        var doc = KdlReader.Read(kdl);

        var result = doc.Nodes[0]["key"];

        await Assert.That(result).IsNotNull();
        await Assert.That(result).IsTypeOf<KdlString>();
        await Assert.That(((KdlString)result!).Value).IsEqualTo("value");
    }

    [Test]
    public async Task Indexer_NonExistingProperty_ReturnsNull()
    {
        var kdl = "node key=\"value\"";
        var doc = KdlReader.Read(kdl);

        var result = doc.Nodes[0]["nonexistent"];

        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task Indexer_DuplicateProperties_ReturnsLastValue()
    {
        var kdl = "node key=\"first\" key=\"second\" key=\"third\"";
        var doc = KdlReader.Read(kdl);

        var result = doc.Nodes[0]["key"];

        await Assert.That(((KdlString)result!).Value).IsEqualTo("third");
    }

    #endregion

    #region Arguments Property Tests

    [Test]
    public async Task Arguments_WithPositionalValues_ReturnsAllArguments()
    {
        var kdl = "node 1 2 3 key=\"value\"";
        var doc = KdlReader.Read(kdl);

        var args = doc.Nodes[0].Arguments.ToList();

        await Assert.That(args).Count().IsEqualTo(3);
    }

    [Test]
    public async Task Arguments_NoArguments_ReturnsEmpty()
    {
        var kdl = "node key=\"value\"";
        var doc = KdlReader.Read(kdl);

        var args = doc.Nodes[0].Arguments.ToList();

        await Assert.That(args).IsEmpty();
    }

    #endregion

    #region Properties Property Tests

    [Test]
    public async Task Properties_WithKeyValuePairs_ReturnsAllProperties()
    {
        var kdl = "node 1 key1=\"val1\" key2=\"val2\"";
        var doc = KdlReader.Read(kdl);

        var props = doc.Nodes[0].Properties.ToList();

        await Assert.That(props).Count().IsEqualTo(2);
        await Assert.That(props[0].Key.Value).IsEqualTo("key1");
        await Assert.That(props[1].Key.Value).IsEqualTo("key2");
    }

    [Test]
    public async Task Properties_NoProperties_ReturnsEmpty()
    {
        var kdl = "node 1 2 3";
        var doc = KdlReader.Read(kdl);

        var props = doc.Nodes[0].Properties.ToList();

        await Assert.That(props).IsEmpty();
    }

    #endregion

    #region HasChildren Property Tests

    [Test]
    public async Task HasChildren_WithChildren_ReturnsTrue()
    {
        var kdl = "parent { child; }";
        var doc = KdlReader.Read(kdl);

        var result = doc.Nodes[0].HasChildren;

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task HasChildren_WithEmptyBlock_ReturnsFalse()
    {
        var kdl = "parent { }";
        var doc = KdlReader.Read(kdl);

        var result = doc.Nodes[0].HasChildren;

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task HasChildren_NoBlock_ReturnsFalse()
    {
        var kdl = "node 123";
        var doc = KdlReader.Read(kdl);

        var result = doc.Nodes[0].HasChildren;

        await Assert.That(result).IsFalse();
    }

    #endregion

    #region TypeAnnotation Tests

    [Test]
    public async Task TypeAnnotation_WhenSet_ReturnsAnnotation()
    {
        var kdl = "(mytype)node 123";
        var doc = KdlReader.Read(kdl);

        var result = doc.Nodes[0].TypeAnnotation;

        await Assert.That(result).IsEqualTo("mytype");
    }

    [Test]
    public async Task TypeAnnotation_NotSet_ReturnsNull()
    {
        var kdl = "node 123";
        var doc = KdlReader.Read(kdl);

        var result = doc.Nodes[0].TypeAnnotation;

        await Assert.That(result).IsNull();
    }

    #endregion

    #region Record Equality Tests

    [Test]
    public async Task Equality_SameListInstance_AreEqual()
    {
        var name = new KdlString("node", StringKind.Bare);
        var arg = new KdlArgument(new KdlNumber("42"));
        var entries = new List<KdlEntry> { arg };

        var node1 = new KdlNode(name) { Entries = entries };
        var node2 = new KdlNode(name) { Entries = entries };

        await Assert.That(node1).IsEqualTo(node2);
    }

    [Test]
    public async Task Equality_DifferentNames_AreNotEqual()
    {
        var node1 = new KdlNode(new KdlString("node1", StringKind.Bare));
        var node2 = new KdlNode(new KdlString("node2", StringKind.Bare));

        await Assert.That(node1).IsNotEqualTo(node2);
    }

    #endregion
}
