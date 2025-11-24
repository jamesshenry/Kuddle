using System.Runtime.CompilerServices;
using Kuddle.AST;

namespace Kuddle.Tests;

public class KdlNodeTests
{
    #region Structural Identity & Equality (Record Semantics)

    [Test]
    public async Task Given_IdenticalNodes_When_Compared_Then_AreEqual()
    {
        // Arrange
        var name = new KdlIdentifier("test");
        var arg = new KdlArgument(new KdlString("value", StringKind.Quoted));
        var node1 = new KdlNode(name)
        {
            Entries = [arg],
            LeadingTrivia = "  ",
            TrailingTrivia = "\n",
        };
        var node2 = new KdlNode(name)
        {
            Entries = [arg],
            LeadingTrivia = "  ",
            TrailingTrivia = "\n",
        };

        // Act & Assert
        await Assert.That(node1).IsEquivalentTo(node2);
    }

    [Test]
    public async Task Given_NodesWithDifferentLeadingTrivia_When_Compared_Then_AreNotEqual()
    {
        // Arrange
        var name = new KdlIdentifier("test");
        var node1 = new KdlNode(name) { LeadingTrivia = "  " };
        var node2 = new KdlNode(name) { LeadingTrivia = "" };

        // Act & Assert
        await Assert.That(node1).IsNotEqualTo(node2);
    }

    [Test]
    public async Task Given_NodesWithDifferentTrailingTrivia_When_Compared_Then_AreNotEqual()
    {
        // Arrange
        var name = new KdlIdentifier("test");
        var node1 = new KdlNode(name) { TrailingTrivia = "\n" };
        var node2 = new KdlNode(name) { TrailingTrivia = "" };

        // Act & Assert
        await Assert.That(node1).IsNotEqualTo(node2);
    }

    [Test]
    public async Task Given_Node_When_ModifiedWithExpression_Then_OriginalIsUnchanged()
    {
        // Arrange
        var original = new KdlNode(new KdlIdentifier("test"));

        // Act
        var modified = original with
        {
            LeadingTrivia = "  ",
        };

        // Assert
        await Assert.That(original.LeadingTrivia).IsEmpty();
        await Assert.That(modified.LeadingTrivia).IsEqualTo("  ");
        await Assert.That(original).IsNotEqualTo(modified);
    }

    #endregion

    #region Node Structure & Entry Management

    [Test]
    public async Task Given_NodeWithInterspersedEntries_When_EntriesAdded_Then_OrderIsPreserved()
    {
        // Arrange
        var node = new KdlNode(new KdlIdentifier("test"));
        var arg1 = new KdlArgument(new KdlString("arg1", StringKind.Quoted));
        var prop1 = new KdlProperty(
            new KdlIdentifier("key1"),
            new KdlString("value1", StringKind.Quoted)
        );
        var arg2 = new KdlArgument(new KdlString("arg2", StringKind.Quoted));
        var prop2 = new KdlProperty(
            new KdlIdentifier("key2"),
            new KdlString("value2", StringKind.Quoted)
        );

        // Act
        node.Entries.Add(arg1);
        node.Entries.Add(prop1);
        node.Entries.Add(arg2);
        node.Entries.Add(prop2);

        // Assert
        await Assert.That(node.Entries.Count).IsEqualTo(4);
        await Assert.That(node.Entries[0]).IsEqualTo(arg1);
        await Assert.That(node.Entries[1]).IsEqualTo(prop1);
        await Assert.That(node.Entries[2]).IsEqualTo(arg2);
        await Assert.That(node.Entries[3]).IsEqualTo(prop2);
    }

    [Test]
    public async Task Given_NodeWithChildren_When_BlockAdded_Then_ChildrenAreAccessible()
    {
        // Arrange
        var child1 = new KdlNode(new KdlIdentifier("child1"));
        var child2 = new KdlNode(new KdlIdentifier("child2"));
        var parent = new KdlNode(new KdlIdentifier("parent"))
        {
            Children = new KdlBlock { Nodes = [child1, child2] },
        };

        // Assert
        await Assert.That(parent.Children).IsNotNull();
        await Assert.That(parent.Children!.Nodes.Count).IsEqualTo(2);
        await Assert.That(parent.Children.Nodes[0]).IsEqualTo(child1);
        await Assert.That(parent.Children.Nodes[1]).IsEqualTo(child2);
    }

    [Test]
    public async Task Given_SkippedEntry_When_AddedToEntries_Then_RawTextIsRetrievable()
    {
        // Arrange
        var node = new KdlNode(new KdlIdentifier("test"));
        var skipped = new KdlSkippedEntry("/* comment */");

        // Act
        node.Entries.Add(skipped);

        // Assert
        await Assert.That(node.Entries.Count).IsEqualTo(1);
        await Assert.That(node.Entries[0]).IsOfType(typeof(KdlSkippedEntry));
        await Assert.That(((KdlSkippedEntry)node.Entries[0]).RawText).IsEqualTo("/* comment */");
    }

    #endregion

    #region Trivia Fidelity

    [Test]
    public async Task Given_KdlObject_When_LeadingTriviaSet_Then_IsStoredCorrectly()
    {
        // Arrange
        var obj = new KdlNode(new KdlIdentifier("test"))
        {
            // Act
            LeadingTrivia = "  \t",
        };

        // Assert
        await Assert.That(obj.LeadingTrivia).IsEqualTo("  \t");
    }

    [Test]
    public async Task Given_KdlObject_When_TrailingTriviaSet_Then_IsStoredCorrectly()
    {
        // Arrange
        var obj = new KdlNode(new KdlIdentifier("test"))
        {
            // Act
            TrailingTrivia = "\n\t ",
        };

        // Assert
        await Assert.That(obj.TrailingTrivia).IsEqualTo("\n\t ");
    }

    [Test]
    public async Task Given_NodesWithDifferentTrivia_When_Compared_Then_AreNotEqual()
    {
        // Arrange
        var node1 = new KdlNode(new KdlIdentifier("test")) { LeadingTrivia = "  " };
        var node2 = new KdlNode(new KdlIdentifier("test")) { LeadingTrivia = "" };

        // Act & Assert
        await Assert.That(node1).IsNotEqualTo(node2);
    }

    #endregion

    #region KdlIdentifier & Type Annotations

    [Test]
    public async Task Given_BareIdentifier_When_Created_Then_RawTextEqualsName()
    {
        // Arrange & Act
        var identifier = new KdlIdentifier("foo");

        // Assert
        await Assert.That(identifier.Name).IsEqualTo("foo");
        await Assert.That(identifier.RawText).IsEqualTo("foo");
    }

    [Test]
    public async Task Given_QuotedIdentifier_When_Created_Then_RawTextIncludesQuotes()
    {
        // Arrange & Act
        var identifier = new KdlIdentifier("foo") { RawText = "\"foo\"" };

        // Assert
        await Assert.That(identifier.Name).IsEqualTo("foo");
        await Assert.That(identifier.RawText).IsEqualTo("\"foo\"");
    }

    [Test]
    public async Task Given_IdentifierWithTypeAnnotation_When_Created_Then_TypeAnnotationIsStored()
    {
        // Arrange & Act
        var identifier = new KdlIdentifier("name") { TypeAnnotation = "type" };

        // Assert
        await Assert.That(identifier.TypeAnnotation).IsEqualTo("type");
    }

    [Test]
    public async Task Given_ValueWithTypeAnnotation_When_Created_Then_TypeAnnotationIsStored()
    {
        // Arrange & Act
        var value = new KdlString("hello", StringKind.Quoted) { TypeAnnotation = "str" };

        // Assert
        await Assert.That(value.TypeAnnotation).IsEqualTo("str");
    }

    #endregion
}
