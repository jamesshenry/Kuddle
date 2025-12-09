using Kuddle.AST;
using Kuddle.Exceptions;
using Kuddle.Validation;

namespace Kuddle.Tests.Validation;

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
            .Throws<KdlValidationException>();

        // Assert
        // TUnit might have different assertions for collections,
        // essentially checking exception.Errors contains the right message
        await Assert.That(exception.Errors).IsNotEmpty();
        await Assert.That(exception.Errors.First().Message).Contains("not a valid 'u8'");
    }

    [Test]
    public async Task Given_StringForIntType_When_Validated_Then_ThrowsMismatchError()
    {
        // Arrange
        var doc = Parse("node (u8)\"not a number\"");

        // Act
        var exception = Assert.Throws<KdlValidationException>(() =>
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
        var exception = Assert.Throws<KdlValidationException>(() =>
            KdlReservedTypeValidator.Validate(doc)
        );

        // Assert
        await Assert.That(exception.Errors.First().Message).Contains("not a valid 'uuid'");
    }

    [Test]
    public async Task Given_NodeNameWithReservedType_When_Validated_Then_IgnoresIt()
    {
        // Arrange
        // (u8) here applies to the NODE NAME "123", not a value.
        // This is valid structure, and the validator should IGNORE strict type checks on names.
        var doc = Parse("node (u8)123");

        // Act & Assert
        await Assert.That(() => KdlReservedTypeValidator.Validate(doc)).ThrowsNothing();
    }

    // Helper to quickly get a doc from your parser
    private static KdlDocument Parse(string input)
    {
        // Assuming you have this set up from the previous steps
        // If KdlParser.Parse runs validation by default, pass 'false' here
        // so we can test the validator manually.
        return KdlReader.Parse(input, KuddleOptions.Default with { ValidateReservedTypes = false });
    }
}
