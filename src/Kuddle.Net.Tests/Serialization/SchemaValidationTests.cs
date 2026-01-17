using Kuddle.Exceptions;
using Kuddle.Serialization;

namespace Kuddle.Tests.Serialization;

public class SchemaValidationTests
{
    [Test]
    public async Task Serialize_DuplicateSlotKeys_ThrowsConfigurationException()
    {
        var model = new DuplicateSlotModel { Name = "test", Title = "test" };

        // Assert Rule 5: Slot Uniqueness
        await Assert.That(() => KdlSerializer.Serialize(model)).Throws<KdlConfigurationException>();
    }

    [Test]
    public async Task Serialize_MisplacedRestArgument_ThrowsConfigurationException()
    {
        var model = new MisplacedRestModel { Items = ["a"], Final = "b" };

        // Assert Rule 9: Argument Ambiguity (Rest must be the last argument)
        await Assert.That(() => KdlSerializer.Serialize(model)).Throws<KdlConfigurationException>();
    }

    [Test]
    public async Task Serialize_FlattenScalar_ThrowsConfigurationException()
    {
        var model = new IllegalFlattenModel { Id = 1 };

        // Assert Rule 16: Illegal Flattening (Flatten=true on non-collections)
        await Assert.That(() => KdlSerializer.Serialize(model)).Throws<KdlConfigurationException>();
    }

    // Models for testing
    public class DuplicateSlotModel
    {
        [KdlProperty("id")]
        public string Name { get; set; } = "";

        [KdlProperty("id")]
        public string Title { get; set; } = "";
    }

    public class MisplacedRestModel
    {
        [KdlArgument(0)]
        public List<string> Items { get; set; } = [];

        [KdlArgument(1)]
        public string Final { get; set; } = "";
    }

    public class IllegalFlattenModel
    {
        [KdlNode("id", Flatten = true)]
        public int Id { get; set; }
    }
}
