using Kuddle.AST;
using Kuddle.Exceptions;
using Kuddle.Serialization;
using Kuddle.Validation;

namespace Kuddle.Tests.Validation;

/// <summary>
/// Tests for KdlReservedTypeValidator validating all reserved type annotations.
/// </summary>
public class ReservedTypeValidatorTests
{
    #region Integer Type Validation (u8, u16, u32, u64, i8, i16, i32, i64)

    [Test]
    public async Task Given_ValidU8_When_Validated_Then_NoExceptionThrown()
    {
        var doc = Parse("node (u8)255");

        await Assert.That(() => KdlReservedTypeValidator.Validate(doc)).ThrowsNothing();
    }

    [Test]
    public async Task Given_OverflowingU8_When_Validated_Then_ThrowsValidationException()
    {
        var doc = Parse("node (u8)256");

        var exception = await Assert
            .That(() => KdlReservedTypeValidator.Validate(doc))
            .Throws<KuddleValidationException>();

        await Assert.That(exception!.Errors).IsNotEmpty();
        await Assert.That(exception.Errors.First().Message).Contains("not a valid 'u8'");
    }

    [Test]
    public async Task Given_NegativeU8_When_Validated_Then_ThrowsValidationException()
    {
        var doc = Parse("node (u8)-1");

        await Assert
            .That(() => KdlReservedTypeValidator.Validate(doc))
            .Throws<KuddleValidationException>();
    }

    [Test]
    [Arguments("u16", 65535)]
    [Arguments("u32", 4294967295)]
    public async Task Given_ValidUnsignedMax_When_Validated_Then_NoExceptionThrown(
        string type,
        long maxValue
    )
    {
        var doc = Parse($"node ({type}){maxValue}");

        await Assert.That(() => KdlReservedTypeValidator.Validate(doc)).ThrowsNothing();
    }

    [Test]
    [Arguments("i8", -128)]
    [Arguments("i8", 127)]
    [Arguments("i16", -32768)]
    [Arguments("i16", 32767)]
    [Arguments("i32", int.MinValue)]
    [Arguments("i32", int.MaxValue)]
    public async Task Given_ValidSignedBoundary_When_Validated_Then_NoExceptionThrown(
        string type,
        long value
    )
    {
        var doc = Parse($"node ({type}){value}");

        await Assert.That(() => KdlReservedTypeValidator.Validate(doc)).ThrowsNothing();
    }

    [Test]
    public async Task Given_OverflowingI8_When_Validated_Then_ThrowsValidationException()
    {
        var doc = Parse("node (i8)128");

        await Assert
            .That(() => KdlReservedTypeValidator.Validate(doc))
            .Throws<KuddleValidationException>();
    }

    [Test]
    public async Task Given_UnderflowingI8_When_Validated_Then_ThrowsValidationException()
    {
        var doc = Parse("node (i8)-129");

        await Assert
            .That(() => KdlReservedTypeValidator.Validate(doc))
            .Throws<KuddleValidationException>();
    }

    #endregion

    #region Float Type Validation (f32, f64)

    [Test]
    public async Task Given_ValidF32_When_Validated_Then_NoExceptionThrown()
    {
        var doc = Parse("node (f32)3.14");

        await Assert.That(() => KdlReservedTypeValidator.Validate(doc)).ThrowsNothing();
    }

    [Test]
    public async Task Given_ValidF64_When_Validated_Then_NoExceptionThrown()
    {
        var doc = Parse("node (f64)3.141592653589793");

        await Assert.That(() => KdlReservedTypeValidator.Validate(doc)).ThrowsNothing();
    }

    [Test]
    public async Task Given_InfinityF32_When_Validated_Then_NoExceptionThrown()
    {
        var doc = Parse("node (f32)#inf");

        await Assert.That(() => KdlReservedTypeValidator.Validate(doc)).ThrowsNothing();
    }

    [Test]
    public async Task Given_NegativeInfinityF64_When_Validated_Then_NoExceptionThrown()
    {
        var doc = Parse("node (f64)#-inf");

        await Assert.That(() => KdlReservedTypeValidator.Validate(doc)).ThrowsNothing();
    }

    [Test]
    public async Task Given_NaNF32_When_Validated_Then_NoExceptionThrown()
    {
        var doc = Parse("node (f32)#nan");

        await Assert.That(() => KdlReservedTypeValidator.Validate(doc)).ThrowsNothing();
    }

    #endregion

    #region UUID Validation

    [Test]
    public async Task Given_ValidUuid_When_Validated_Then_NoExceptionThrown()
    {
        var uuid = Guid.NewGuid().ToString();
        var doc = Parse($"node (uuid)\"{uuid}\"");

        await Assert.That(() => KdlReservedTypeValidator.Validate(doc)).ThrowsNothing();
    }

    [Test]
    public async Task Given_InvalidUuid_When_Validated_Then_ThrowsError()
    {
        var doc = Parse("node (uuid)\"im-not-a-uuid\"");

        var exception = await Assert
            .That(() => KdlReservedTypeValidator.Validate(doc))
            .Throws<KuddleValidationException>();

        await Assert.That(exception!.Errors.First().Message).Contains("not a valid 'uuid'");
    }

    [Test]
    public async Task Given_UuidAsNumber_When_Validated_Then_ThrowsTypeMismatch()
    {
        var doc = Parse("node (uuid)12345");

        await Assert
            .That(() => KdlReservedTypeValidator.Validate(doc))
            .Throws<KuddleValidationException>();
    }

    #endregion

    #region Date/Time Validation

    [Test]
    public async Task Given_ValidDateTime_When_Validated_Then_NoExceptionThrown()
    {
        var doc = Parse("node (date-time)\"2025-01-15T10:30:00Z\"");

        await Assert.That(() => KdlReservedTypeValidator.Validate(doc)).ThrowsNothing();
    }

    [Test]
    public async Task Given_ValidDate_When_Validated_Then_NoExceptionThrown()
    {
        var doc = Parse("node (date)\"2025-01-15\"");

        await Assert.That(() => KdlReservedTypeValidator.Validate(doc)).ThrowsNothing();
    }

    [Test]
    public async Task Given_ValidTime_When_Validated_Then_NoExceptionThrown()
    {
        var doc = Parse("node (time)\"10:30:00\"");

        await Assert.That(() => KdlReservedTypeValidator.Validate(doc)).ThrowsNothing();
    }

    [Test]
    public async Task Given_InvalidDateTime_When_Validated_Then_ThrowsError()
    {
        var doc = Parse("node (date-time)\"not-a-date\"");

        await Assert
            .That(() => KdlReservedTypeValidator.Validate(doc))
            .Throws<KuddleValidationException>();
    }

    [Test]
    public async Task Given_InvalidDate_When_Validated_Then_ThrowsError()
    {
        var doc = Parse("node (date)\"not-a-valid-date\"");

        await Assert
            .That(() => KdlReservedTypeValidator.Validate(doc))
            .Throws<KuddleValidationException>();
    }

    #endregion

    #region Type Mismatch Tests

    [Test]
    public async Task Given_StringForIntType_When_Validated_Then_ThrowsMismatchError()
    {
        var doc = Parse("node (u8)\"not a number\"");

        var exception = await Assert
            .That(() => KdlReservedTypeValidator.Validate(doc))
            .Throws<KuddleValidationException>();

        await Assert.That(exception!.Errors.First().Message).Contains("Expected a Number");
    }

    [Test]
    public async Task Given_NumberForUuidType_When_Validated_Then_ThrowsMismatchError()
    {
        var doc = Parse("node (uuid)12345");

        await Assert
            .That(() => KdlReservedTypeValidator.Validate(doc))
            .Throws<KuddleValidationException>();
    }

    [Test]
    public async Task Given_BoolForIntType_When_Validated_Then_ThrowsMismatchError()
    {
        var doc = Parse("node (i32)#true");

        await Assert
            .That(() => KdlReservedTypeValidator.Validate(doc))
            .Throws<KuddleValidationException>();
    }

    #endregion

    #region Node Type Annotation Tests

    [Test]
    public async Task Given_NodeWithReservedTypeAnnotation_When_Validated_Then_ValidatesCorrectly()
    {
        var doc = Parse("(u8)node 123");

        await Assert.That(() => KdlReservedTypeValidator.Validate(doc)).ThrowsNothing();
    }

    [Test]
    public async Task Given_ValidValueWithTypeAnnotation_When_Validated_Then_NoExceptionThrown()
    {
        var doc = Parse("node (u8)123");

        await Assert.That(() => KdlReservedTypeValidator.Validate(doc)).ThrowsNothing();
    }

    #endregion

    #region Validation Disabled Tests

    [Test]
    public async Task Given_InvalidType_When_ValidationDisabled_Then_NoExceptionThrown()
    {
        var kdl = "node (u8)999";
        var doc = KdlReader.Read(
            kdl,
            KdlReaderOptions.Default with
            {
                ValidateReservedTypes = false,
            }
        );

        // Validation is skipped during parsing, manual validation would still fail
        await Assert.That(doc.Nodes).Count().IsEqualTo(1);
    }

    #endregion

    #region Multiple Errors Tests

    [Test]
    public async Task Given_MultipleInvalidValues_When_Validated_Then_CollectsAllErrors()
    {
        var doc = Parse("node (u8)999 (uuid)\"invalid\" (i8)500");

        var exception = await Assert
            .That(() => KdlReservedTypeValidator.Validate(doc))
            .Throws<KuddleValidationException>();

        await Assert.That(exception!.Errors.Count()).IsGreaterThanOrEqualTo(3);
    }

    #endregion

    #region Helper Methods

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

    #endregion
}
