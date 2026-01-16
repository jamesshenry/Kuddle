using Kuddle.AST;
using Kuddle.Serialization;

namespace Kuddle.Tests.Types;

/// <summary>
/// Tests for KdlNull construction and behavior.
/// </summary>
public class KdlNullTests
{
    #region Construction Tests

    [Test]
    public async Task Constructor_CreatesNullValue()
    {
        var sut = new KdlNull();

        await Assert.That(sut).IsNotNull();
    }

    [Test]
    public async Task StaticNull_ReturnsKdlNull()
    {
        var sut = KdlValue.Null;

        await Assert.That(sut).IsTypeOf<KdlNull>();
    }

    #endregion

    #region Parsing Tests

    [Test]
    public async Task Parse_NullKeyword_CreatesKdlNull()
    {
        var kdl = "node #null";
        var doc = KdlReader.Read(kdl);

        var arg = doc.Nodes[0].Arguments.First();

        await Assert.That(arg).IsTypeOf<KdlNull>();
    }

    [Test]
    public async Task Parse_NullAsProperty_Works()
    {
        var kdl = "node value=#null";
        var doc = KdlReader.Read(kdl);

        var value = doc.Nodes[0]["value"];

        await Assert.That(value).IsTypeOf<KdlNull>();
    }

    #endregion

    #region Record Equality Tests

    [Test]
    public async Task Equality_TwoNulls_AreEqual()
    {
        var null1 = new KdlNull();
        var null2 = new KdlNull();

        await Assert.That(null1).IsEqualTo(null2);
    }

    [Test]
    public async Task Equality_NullAndStaticNull_AreEqual()
    {
        var null1 = new KdlNull();
        KdlNull null2 = KdlValue.Null;
        await Assert.That(null1).IsEqualTo(null2);
    }

    #endregion

    #region TypeAnnotation Tests

    [Test]
    public async Task TypeAnnotation_CanBeSet()
    {
        var sut = new KdlNull() { TypeAnnotation = "mytype" };

        await Assert.That(sut.TypeAnnotation).IsEqualTo("mytype");
    }

    #endregion
}
