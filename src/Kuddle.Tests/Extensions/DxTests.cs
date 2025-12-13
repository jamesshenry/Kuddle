using Kuddle.AST;
using Kuddle.Extensions;
using Kuddle.Parser;

namespace Kuddle.Tests.Extensions;

public class DxTests
{
    [Test]
    public async Task TryGet_Int_Success()
    {
        var doc = KuddleReader.Parse("node 123");
        var val = doc.Nodes[0].Arg(0);

        bool success = val.TryGetInt(out int result);

        await Assert.That(success).IsTrue();
        await Assert.That(result).IsEqualTo(123);
    }

    [Test]
    public async Task TryGet_Int_Failure_WrongType()
    {
        var doc = KuddleReader.Parse("node \"hello\"");
        var val = doc.Nodes[0].Arg(0);

        bool success = val.TryGetInt(out int result);

        await Assert.That(success).IsFalse();
        await Assert.That(result).IsEqualTo(0); // Default
    }

    [Test]
    public async Task TryGet_Int_Failure_Overflow()
    {
        // Value larger than Int32
        var doc = KuddleReader.Parse("node 9999999999");
        var val = doc.Nodes[0].Arg(0);

        bool success = val.TryGetInt(out int result);

        await Assert.That(success).IsFalse();
    }

    [Test]
    public async Task TryGet_Prop_Navigation_Success()
    {
        var doc = KuddleReader.Parse("server port=8080");
        var node = doc.Nodes[0];

        // Combine finding the prop and converting it
        var propVal = node.Prop("port");

        if (propVal.TryGetInt(out int port))
        {
            await Assert.That(port).IsEqualTo(8080);
        }
        else
        {
            Assert.Fail("Failed to get port");
        }
    }

    [Test]
    public async Task TryGet_Prop_Navigation_Missing()
    {
        var doc = KuddleReader.Parse("server host=\"localhost\"");
        var node = doc.Nodes[0];

        var propVal = node.Prop("port"); // Returns KdlNull

        bool success = propVal.TryGetInt(out _);
        await Assert.That(success).IsFalse();
    }

    [Test]
    public async Task TryGetUuid_ValidGuidString_ReturnsTrue()
    {
        var expected = Guid.NewGuid();
        var kdl = $"node \"{expected}\""; // e.g. "f81d4fae-7dec-11d0-a765-00a0c91e6bf6"
        var doc = KuddleReader.Parse(kdl);
        var val = doc.Nodes[0].Arg(0);

        bool success = val.TryGetUuid(out var result);

        await Assert.That(success).IsTrue();
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    public async Task TryGetUuid_InvalidString_ReturnsFalse()
    {
        var doc = KuddleReader.Parse("node \"not-a-guid\"");
        var val = doc.Nodes[0].Arg(0);

        bool success = val.TryGetUuid(out var result);

        await Assert.That(success).IsFalse();
        await Assert.That(result).IsEqualTo(Guid.Empty);
    }

    [Test]
    public async Task TryGetUuid_FromAnnotatedValue_ReturnsTrue()
    {
        // KDL 2.0 Spec: (uuid)"..."
        var expected = Guid.NewGuid();
        var kdl = $"node (uuid)\"{expected}\"";
        var doc = KuddleReader.Parse(kdl);
        var val = doc.Nodes[0].Arg(0);

        bool success = val.TryGetUuid(out var result);

        await Assert.That(success).IsTrue();
        await Assert.That(result).IsEqualTo(expected);

        // Verify annotation didn't interfere
        await Assert.That(val.TypeAnnotation).IsEqualTo("uuid");
    }

    [Test]
    public async Task Factory_FromGuid_CreatesAnnotatedString()
    {
        var guid = Guid.NewGuid();
        var val = KdlValue.From(guid);

        // 1. Check Runtime Type
        await Assert.That(val).IsOfType(typeof(KdlString));

        // 2. Check Content
        await Assert.That(val.Value).IsEqualTo(guid.ToString());

        // 3. Check Type Annotation (Crucial for KDL semantic correctness)
        await Assert.That(val.TypeAnnotation).IsEqualTo("uuid");
    }

    [Test]
    public async Task TryGetDateTime_ValidIso8601_ReturnsTrue()
    {
        var now = DateTimeOffset.UtcNow;
        // Round-trip format "O" is standard for KDL/JSON
        var kdl = $"node \"{now:O}\"";
        var doc = KuddleReader.Parse(kdl);
        var val = doc.Nodes[0].Arg(0);

        bool success = val.TryGetDateTime(out var result);

        await Assert.That(success).IsTrue();
        // Compare ticks to ensure precision is kept
        await Assert.That(result).IsEqualTo(now);
    }

    [Test]
    public async Task TryGetDateTime_DateOnly_ReturnsTrue()
    {
        // YYYY-MM-DD
        var kdl = "node \"2023-10-25\"";
        var doc = KuddleReader.Parse(kdl);
        var val = doc.Nodes[0].Arg(0);

        bool success = val.TryGetDateTime(out var result);

        await Assert.That(success).IsTrue();
        await Assert.That(result.Year).IsEqualTo(2023);
        await Assert.That(result.Month).IsEqualTo(10);
        await Assert.That(result.Day).IsEqualTo(25);
    }

    [Test]
    public async Task TryGetDateTime_InvalidString_ReturnsFalse()
    {
        var doc = KuddleReader.Parse("node \"tomorrow\"");
        var val = doc.Nodes[0].Arg(0);

        bool success = val.TryGetDateTime(out var result);

        await Assert.That(success).IsFalse();
        await Assert.That(result).IsEqualTo(default(DateTimeOffset));
    }

    [Test]
    public async Task Factory_FromDateTime_CreatesAnnotatedString()
    {
        var date = new DateTimeOffset(2023, 12, 25, 10, 0, 0, TimeSpan.Zero);
        var val = KdlValue.From(date);

        // 1. Check Runtime Type
        await Assert.That(val).IsOfType(typeof(KdlString));

        // 2. Check Content (ISO 8601)
        await Assert.That(val.Value).IsEqualTo(date.ToString("O"));

        // 3. Check Type Annotation
        await Assert.That(val.TypeAnnotation).IsEqualTo("date-time");
    }
}
