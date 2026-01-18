using Kuddle.Serialization;

namespace Kuddle.Tests.Serialization;

public class FidelityAndFormatTests
{
    [Test]
    public async Task Serialize_LineNormalization_UsesLF()
    {
        var model = new { Text = "Line1\r\nLine2" };
        var kdl = KdlSerializer.Serialize(model);

        // Assert Rule 22: Line normalization to LF
        await Assert.That(kdl).DoesNotContain("\r\n");
        await Assert.That(kdl).Contains("\n");
    }

    [Test]
    public async Task Serialize_SpecialCharacters_UsesRawStrings()
    {
        var model = new { Raw = "String with \"quotes\" and \\slash" };
        var options = new KdlSerializerOptions { StringStyle = KdlStringStyle.PreferRaw };

        var kdl = KdlSerializer.Serialize(model, options);

        // KDL v2: Expected syntax is #"..."# or ##"..."##
        // If the string contains a quote, it needs at least one #.
        // If it contains "#, it needs ##.
        await Assert.That(kdl).Contains("#\"String with \"quotes\" and \\slash\"#");
    }

    [Test]
    public async Task Deserialize_Slashdash_ShiftsArgumentIndices()
    {
        // Prove Rule 25: If /- works, "actual" becomes Argument 0.
        // If /- failed, "ignored" would be Argument 0.
        var kdl = "node /- \"ignored\" \"actual\"";

        var result = KdlSerializer.Deserialize<ArgModel>(kdl);

        await Assert.That(result.Value).IsEqualTo("actual");
    }

    [Test]
    public async Task Deserialize_Slashdash_RemovesPropertyFromExtensionData()
    {
        // Prove Rule 25: A commented property should not appear in ExtensionData.
        const string kdl = """
node /- key="hidden" visible="shown"
""";

        var result = KdlSerializer.Deserialize<ExtensionModel>(kdl);

        await Assert.That(result.Data).DoesNotContainKey("key");
        await Assert.That(result.Visible).IsEqualTo("shown");
    }

    [Test]
    public async Task Deserialize_Slashdash_PreventsPropertyOverride()
    {
        // Rule 6 says "Last Wins", but Rule 25 says slashdash removes the token.
        // Therefore, "first" should be the final value.
        var kdl = "node val=\"first\" /- val=\"second\"";

        var result = KdlSerializer.Deserialize<PropModel>(kdl);

        await Assert.That(result.Val).IsEqualTo("first");
    }

    // --- Models ---

    public class ArgModel
    {
        [KdlArgument(0)]
        public string Value { get; set; } = "";
    }

    public class PropModel
    {
        [KdlProperty("val")]
        public string Val { get; set; } = "";
    }

    public class ExtensionModel
    {
        [KdlProperty]
        public string Visible { get; set; } = "";

        [KdlExtensionData]
        public Dictionary<string, object> Data { get; set; } = [];
    }
}
