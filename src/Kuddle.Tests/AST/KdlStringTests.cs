using Kuddle.AST;

namespace Kuddle.Tests.AST;

public class KdlStringTests
{
    #region Constructor and Value Property Tests

    [Test]
    [Arguments("world", StringKind.Quoted)]
    [Arguments("", StringKind.Raw)]
    [Arguments("string\nwith\nnewlines", StringKind.Quoted)]
    public async Task Constructor_SetsValueAndTypeCorrectly(string input, StringKind type)
    {
        var sut = new KdlString(input, type);
        await Assert.That(sut.Value).IsEqualTo(input);
        await Assert.That(sut.Kind).IsEqualTo(type);
    }

    #endregion

    #region Equality Tests

    [Test]
    public async Task Equals_ReturnsTrueForSameValueAndType()
    {
        var sut1 = new KdlString("test", StringKind.Quoted);
        var sut2 = new KdlString("test", StringKind.Quoted);
        await Assert.That(sut1).IsEqualTo(sut2);
    }

    [Test]
    public async Task Equals_ReturnsFalseForDifferentValue()
    {
        var sut1 = new KdlString("test1", StringKind.Quoted);
        var sut2 = new KdlString("test2", StringKind.Quoted);
        await Assert.That(sut1).IsNotEqualTo(sut2);
    }

    [Test]
    public async Task Equals_ReturnsFalseForDifferentType()
    {
        var sut1 = new KdlString("test", StringKind.Quoted);
        var sut2 = new KdlString("test", StringKind.Bare);
        await Assert.That(sut1).IsNotEqualTo(sut2);
    }

    [Test]
    public async Task Equals_ReturnsFalseForNull()
    {
        var sut = new KdlString("test", StringKind.Quoted);
        await Assert.That(sut.Equals(null)).IsFalse();
    }

    #endregion

    #region ToString Tests

    [Test]
    [Arguments("hello", StringKind.Bare)]
    [Arguments("world", StringKind.Quoted)]
    [Arguments("", StringKind.Raw)]
    public async Task ToString_ReturnsValue(string input, StringKind type)
    {
        var sut = new KdlString(input, type);
        await Assert.That(sut.ToString()).IsEqualTo(input);
    }

    #endregion

    #region TypeAnnotation Tests

    [Test]
    public async Task TypeAnnotation_CanBeSet()
    {
        var sut = new KdlString("test", StringKind.Quoted) { TypeAnnotation = "custom" };
        await Assert.That(sut.TypeAnnotation).IsEqualTo("custom");
    }

    [Test]
    public async Task TypeAnnotation_DefaultsToNull()
    {
        var sut = new KdlString("test", StringKind.Quoted);
        await Assert.That(sut.TypeAnnotation).IsNull();
    }

    #endregion

    #region HashCode Tests

    [Test]
    public async Task GetHashCode_ReturnsSameForEqualValuesAndTypes()
    {
        var sut1 = new KdlString("test", StringKind.Quoted);
        var sut2 = new KdlString("test", StringKind.Quoted);
        await Assert.That(sut1.GetHashCode()).IsEqualTo(sut2.GetHashCode());
    }

    [Test]
    public async Task GetHashCode_ReturnsDifferentForDifferentValues()
    {
        var sut1 = new KdlString("test1", StringKind.Quoted);
        var sut2 = new KdlString("test2", StringKind.Quoted);
        await Assert.That(sut1.GetHashCode()).IsNotEqualTo(sut2.GetHashCode());
    }

    [Test]
    public async Task GetHashCode_ReturnsDifferentForDifferentTypes()
    {
        var sut1 = new KdlString("test", StringKind.Quoted);
        var sut2 = new KdlString("test", StringKind.Bare);
        await Assert.That(sut1.GetHashCode()).IsNotEqualTo(sut2.GetHashCode());
    }

    #endregion

    #region Edge Case Tests

    [Test]
    public async Task EmptyString_HandlesCorrectly()
    {
        var sut = new KdlString("", StringKind.Quoted);
        await Assert.That(sut.Value).IsEqualTo("");
        await Assert.That(sut.ToString()).IsEqualTo("");
        await Assert.That(sut.Kind).IsEqualTo(StringKind.Quoted);
    }

    [Test]
    public async Task StringWithSpecialCharacters_HandlesCorrectly()
    {
        var input = "special\tchars\n\"quotes\"";
        var sut = new KdlString(input, StringKind.Raw);
        await Assert.That(sut.Value).IsEqualTo(input);
        await Assert.That(sut.Kind).IsEqualTo(StringKind.Raw);
    }

    #endregion
}
