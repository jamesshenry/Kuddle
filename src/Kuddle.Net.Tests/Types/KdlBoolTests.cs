using Kuddle.AST;
using Kuddle.Serialization;

namespace Kuddle.Tests.Types;

/// <summary>
/// Tests for KdlBool construction and behavior.
/// </summary>
public class KdlBoolTests
{
    #region Construction Tests

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task Constructor_SetsValue(bool input)
    {
        var sut = new KdlBool(input);

        await Assert.That(sut.Value).IsEqualTo(input);
    }

    #endregion

    #region Parsing Tests

    [Test]
    public async Task Parse_TrueKeyword_CreatesBoolTrue()
    {
        var kdl = "node #true";
        var doc = KdlReader.Read(kdl);

        var arg = doc.Nodes[0].Arguments.First();

        await Assert.That(arg).IsTypeOf<KdlBool>();
        await Assert.That(((KdlBool)arg).Value).IsTrue();
    }

    [Test]
    public async Task Parse_FalseKeyword_CreatesBoolFalse()
    {
        var kdl = "node #false";
        var doc = KdlReader.Read(kdl);

        var arg = doc.Nodes[0].Arguments.First();

        await Assert.That(arg).IsTypeOf<KdlBool>();
        await Assert.That(((KdlBool)arg).Value).IsFalse();
    }

    [Test]
    public async Task Parse_BoolAsProperty_Works()
    {
        var kdl = "node enabled=#true disabled=#false";
        var doc = KdlReader.Read(kdl);

        var enabled = doc.Nodes[0]["enabled"];
        var disabled = doc.Nodes[0]["disabled"];

        await Assert.That(((KdlBool)enabled!).Value).IsTrue();
        await Assert.That(((KdlBool)disabled!).Value).IsFalse();
    }

    #endregion

    #region Record Equality Tests

    [Test]
    public async Task Equality_SameValue_AreEqual()
    {
        var bool1 = new KdlBool(true);
        var bool2 = new KdlBool(true);

        await Assert.That(bool1).IsEqualTo(bool2);
    }

    [Test]
    public async Task Equality_DifferentValue_AreNotEqual()
    {
        var bool1 = new KdlBool(true);
        var bool2 = new KdlBool(false);

        await Assert.That(bool1).IsNotEqualTo(bool2);
    }

    #endregion

    #region TypeAnnotation Tests

    [Test]
    public async Task TypeAnnotation_CanBeSet()
    {
        var sut = new KdlBool(true) { TypeAnnotation = "mybool" };

        await Assert.That(sut.TypeAnnotation).IsEqualTo("mybool");
    }

    #endregion
}
