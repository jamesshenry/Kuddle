using Kuddle.AST;
using Kuddle.Exceptions;
using Kuddle.Serialization;
using Kuddle.Validation;

namespace Kuddle.Tests.Validation;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

public class ReservedTypeValidatorTests
{
    [Test]
    public async Task Given_ValidU8_When_Validated_Then_NoExceptionThrown()
    {
        // Arrange
        var doc = Parse("node 255");

        // Act & Assert
        await Assert.That(() => KdlReservedTypeValidator.Validate(doc)).ThrowsNothing();
    }

    [Test]
    public async Task Given_OverflowingU8_When_Validated_Then_ThrowsValidationException()
    {
        // Arrange
        var doc = Parse("node (u8)256");

        // Act
        var exception = await Assert
            .That(() => KdlReservedTypeValidator.Validate(doc))
            .Throws<KuddleValidationException>();

        // Assert
        await Assert.That(exception.Errors).IsNotEmpty();
        await Assert.That(exception.Errors.First().Message).Contains("not a valid 'u8'");
    }

    [Test]
    public async Task Given_StringForIntType_When_Validated_Then_ThrowsMismatchError()
    {
        // Arrange
        var doc = Parse("node (u8)\"not a number\"");

        // Act
        var exception = Assert.Throws<KuddleValidationException>(() =>
            KdlReservedTypeValidator.Validate(doc)
        );

        // Assert
        await Assert.That(exception.Errors.First().Message).Contains("Expected a Number");
    }

    [Test]
    public async Task Given_InvalidUuid_When_Validated_Then_ThrowsError()
    {
        // Arrange
        var doc = Parse("node (uuid)\"im-not-a-uuid\"");

        // Act
        var exception = Assert.Throws<KuddleValidationException>(() =>
            KdlReservedTypeValidator.Validate(doc)
        );

        // Assert
        await Assert.That(exception.Errors.First().Message).Contains("not a valid 'uuid'");
    }

    [Test]
    public async Task Given_NodeNameWithReservedType_When_Validated_Then_IgnoresIt()
    {
        // Arrange
        var doc = Parse("node (u8)123");

        // Act & Assert
        await Assert.That(() => KdlReservedTypeValidator.Validate(doc)).ThrowsNothing();
    }

    private static KdlDocument Parse(string input)
    {
        return KdlReader.Read(
            input,
            KdlReaderOptions.Default with
            {
                ValidateReservedTypes = false,
            }
        );
    }
}
#pragma warning restore CS8602 // Dereference of a possibly null reference.
