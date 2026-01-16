using Kuddle.AST;
using Kuddle.Serialization;

namespace Kuddle.Tests.Types;

/// <summary>
/// Tests for KdlBlock construction and behavior.
/// </summary>
public class KdlBlockTests
{
    #region Construction Tests

    [Test]
    public async Task Constructor_CreatesEmptyBlock()
    {
        var sut = new KdlBlock();

        await Assert.That(sut.Nodes).IsEmpty();
    }

    [Test]
    public async Task Constructor_WithNodes_StoresNodes()
    {
        var child1 = new KdlNode(new KdlString("child1", StringKind.Bare));
        var child2 = new KdlNode(new KdlString("child2", StringKind.Bare));

        var sut = new KdlBlock { Nodes = [child1, child2] };

        await Assert.That(sut.Nodes).Count().IsEqualTo(2);
    }

    #endregion

    #region Parsing Tests

    [Test]
    public async Task Parse_NodeWithBlock_CreatesKdlBlock()
    {
        var kdl = """
            parent {
                child1
                child2
            }
            """;
        var doc = KdlReader.Read(kdl);

        var block = doc.Nodes[0].Children;

        await Assert.That(block).IsNotNull();
        await Assert.That(block!.Nodes).Count().IsEqualTo(2);
    }

    [Test]
    public async Task Parse_EmptyBlock_CreatesEmptyKdlBlock()
    {
        var kdl = "parent { }";
        var doc = KdlReader.Read(kdl);

        var block = doc.Nodes[0].Children;

        await Assert.That(block).IsNotNull();
        await Assert.That(block!.Nodes).IsEmpty();
    }

    [Test]
    public async Task Parse_NestedBlocks_CreatesNestedStructure()
    {
        var kdl = """
            root {
                level1 {
                    level2 {
                        leaf
                    }
                }
            }
            """;
        var doc = KdlReader.Read(kdl);

        var level1 = doc.Nodes[0].Children!.Nodes[0];
        var level2 = level1.Children!.Nodes[0];
        var leaf = level2.Children!.Nodes[0];

        await Assert.That(leaf.Name.Value).IsEqualTo("leaf");
    }

    #endregion

    #region Record Equality Tests

    [Test]
    public async Task Equality_SameListInstance_AreEqual()
    {
        var child = new KdlNode(new KdlString("child", StringKind.Bare));
        var nodes = new List<KdlNode> { child };
        var block1 = new KdlBlock { Nodes = nodes };
        var block2 = new KdlBlock { Nodes = nodes };

        await Assert.That(block1).IsEqualTo(block2);
    }

    [Test]
    public async Task Equality_DifferentListInstances_AreNotEqual()
    {
        // Records with List<T> use reference equality for the list
        var child = new KdlNode(new KdlString("child", StringKind.Bare));
        var block1 = new KdlBlock { Nodes = [child] };
        var block2 = new KdlBlock { Nodes = [child] };

        await Assert.That(block1).IsNotEqualTo(block2);
    }

    [Test]
    public async Task Equality_DifferentNodes_AreNotEqual()
    {
        var block1 = new KdlBlock
        {
            Nodes = [new KdlNode(new KdlString("child1", StringKind.Bare))],
        };
        var block2 = new KdlBlock
        {
            Nodes = [new KdlNode(new KdlString("child2", StringKind.Bare))],
        };

        await Assert.That(block1).IsNotEqualTo(block2);
    }

    #endregion
}
