using Kuddle.Serialization;

namespace Kuddle.Tests.Serialization;

public class AdvancedMappingTests
{
    [Test]
    public async Task Deserialize_LastWinsRule_RespectsSpec()
    {
        var kdl = "node key=\"first\" key=\"last\"";
        var result = KdlSerializer.Deserialize<SimpleModel>(kdl);

        // Assert Rule 6: Last property wins
        await Assert.That(result.Key).IsEqualTo("last");
    }

    [Test]
    public async Task Deserialize_RestArgument_CapturesRemainder()
    {
        var kdl = "node \"arg1\" \"arg2\" \"arg3\"";
        var result = KdlSerializer.Deserialize<RestArgModel>(kdl);

        // Assert Rule 8: Rest arguments
        await Assert.That(result.First).IsEqualTo("arg1");
        await Assert.That(result.Rest).IsEquivalentTo(new[] { "arg2", "arg3" });
    }

    [Test]
    public async Task Deserialize_StrictMapping_IgnoreMismatchedSlots()
    {
        var kdl = "node \"pos0_val\" id=\"prop_val\"";
        var result = KdlSerializer.Deserialize<StrictMappingModel>(kdl);

        // Assert Rule 19: Arguments do not bleed into properties even if named the same
        await Assert.That(result.Id).IsEqualTo("prop_val");
    }

    [Test]
    public async Task RoundTrip_ExtensionData_PreservesUnmappedNodes()
    {
        var kdl = "root mapped=\"val\" extra_prop=\"123\" { unknown_child; }";
        var result = KdlSerializer.Deserialize<ExtensionModel>(kdl);

        // Assert Rule 20: Extension Data
        await Assert.That(result.Mapped).IsEqualTo("val");
        await Assert.That(result.Data).ContainsKey("extra_prop");
        await Assert.That(result.Data).ContainsKey("unknown_child");
    }

    // Models
    public class SimpleModel
    {
        [KdlProperty("key")]
        public string? Key { get; set; }
    }

    public class RestArgModel
    {
        [KdlArgument(0)]
        public string? First { get; set; }

        [KdlArgument(1)]
        public List<string> Rest { get; set; } = [];
    }

    public class StrictMappingModel
    {
        [KdlProperty("id")]
        public string? Id { get; set; }
    }

    public class ExtensionModel
    {
        [KdlProperty]
        public string? Mapped { get; set; }

        [KdlExtensionData]
        public Dictionary<string, object> Data { get; set; } = [];
    }
}
