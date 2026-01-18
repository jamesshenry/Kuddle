using Kuddle.AST;

namespace Kuddle.Tests.Types;

/// <summary>
/// Tests for KdlValue.From() factory methods.
/// </summary>
public class KdlValueFactoryTests
{
    #region String Factory Tests

    [Test]
    public async Task From_String_CreatesKdlString()
    {
        var result = KdlValue.From("hello");

        await Assert.That(result).IsTypeOf<KdlString>();
        await Assert.That(result.Value).IsEqualTo("hello");
    }

    [Test]
    public async Task From_StringWithWhitespace_UsesQuotedKind()
    {
        var result = KdlValue.From("hello world");

        await Assert.That(result.Kind).IsEqualTo(StringKind.Quoted);
    }

    [Test]
    public async Task From_StringNoWhitespace_UsesBareKind()
    {
        var result = KdlValue.From("hello");

        await Assert.That(result.Kind).IsEqualTo(StringKind.Bare);
    }

    [Test]
    public async Task From_StringWithExplicitKind_UsesSpecifiedKind()
    {
        var result = KdlValue.From("hello", StringKind.Raw);

        await Assert.That(result.Kind).IsEqualTo(StringKind.Raw);
    }

    #endregion

    #region Numeric Factory Tests

    [Test]
    public async Task From_Int_CreatesKdlNumber()
    {
        var result = KdlValue.From(42);

        await Assert.That(result).IsTypeOf<KdlNumber>();
        await Assert.That(result.ToInt32()).IsEqualTo(42);
    }

    [Test]
    public async Task From_NegativeInt_CreatesKdlNumber()
    {
        var result = KdlValue.From(-123);

        await Assert.That(result.ToInt32()).IsEqualTo(-123);
    }

    [Test]
    public async Task From_Long_CreatesKdlNumber()
    {
        var result = KdlValue.From(9223372036854775807L);

        await Assert.That(result.ToInt64()).IsEqualTo(long.MaxValue);
    }

    [Test]
    public async Task From_Double_CreatesKdlNumber()
    {
        var result = KdlValue.From(3.14159);

        await Assert.That(result.ToDouble()).IsEqualTo(3.14159).Within(0.00001);
    }

    [Test]
    public async Task From_Decimal_CreatesKdlNumber()
    {
        var result = KdlValue.From(123.456m);

        await Assert.That(result.ToDecimal()).IsEqualTo(123.456m);
    }

    #endregion

    #region Boolean Factory Tests

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task From_Bool_CreatesKdlBool(bool input)
    {
        var result = KdlValue.From(input);

        await Assert.That(result).IsTypeOf<KdlBool>();
        await Assert.That(result.Value).IsEqualTo(input);
    }

    #endregion

    #region Guid Factory Tests

    [Test]
    public async Task From_Guid_CreatesKdlStringWithUuidAnnotation()
    {
        var guid = Guid.NewGuid();

        var result = KdlValue.From(guid);

        await Assert.That(result).IsTypeOf<KdlString>();
        await Assert.That(result.TypeAnnotation).IsEqualTo("uuid");
        await Assert.That(result.Value).IsEqualTo(guid.ToString());
    }

    #endregion

    #region DateTime Factory Tests

    [Test]
    public async Task From_DateTime_CreatesKdlStringWithDateTimeAnnotation()
    {
        var date = new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Utc);

        var result = KdlValue.From(date);

        await Assert.That(result).IsTypeOf<KdlString>();
        await Assert.That(result.TypeAnnotation).IsEqualTo("date-time");
        await Assert.That(result.Value).Contains("2025-01-15");
    }

    [Test]
    public async Task From_DateTimeOffset_CreatesKdlStringWithDateTimeAnnotation()
    {
        var date = new DateTimeOffset(2025, 1, 15, 10, 30, 0, TimeSpan.FromHours(-5));

        var result = KdlValue.From(date);

        await Assert.That(result.TypeAnnotation).IsEqualTo("date-time");
        await Assert.That(result.Value).Contains("2025-01-15");
    }

    [Test]
    public async Task From_DateOnly_CreatesKdlStringWithDateAnnotation()
    {
        var date = new DateOnly(2025, 12, 25);

        var result = KdlValue.From(date);

        await Assert.That(result.TypeAnnotation).IsEqualTo("date");
    }

    [Test]
    public async Task From_TimeOnly_CreatesKdlStringWithTimeAnnotation()
    {
        var time = new TimeOnly(14, 30, 0);

        var result = KdlValue.From(time);

        await Assert.That(result.TypeAnnotation).IsEqualTo("time");
    }

    #endregion

    #region Enum Factory Tests

    [Test]
    public async Task From_Enum_CreatesKdlStringWithBareKind()
    {
        var result = KdlValue.From(DayOfWeek.Monday);

        await Assert.That(result).IsTypeOf<KdlString>();
        await Assert.That(result.Value).IsEqualTo("Monday");
        await Assert.That(result.Kind).IsEqualTo(StringKind.Bare);
    }

    #endregion

    #region Static Null Property Tests

    [Test]
    public async Task Null_ReturnsKdlNullInstance()
    {
        var result = KdlValue.Null;

        await Assert.That(result).IsTypeOf<KdlNull>();
    }

    [Test]
    public async Task Null_MultipleCalls_ReturnsSameInstance()
    {
        var null1 = KdlValue.Null;
        var null2 = KdlValue.Null;

        await Assert.That(null1).IsEqualTo(null2);
    }

    #endregion
}
