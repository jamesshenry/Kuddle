using Kuddle.Exceptions;
using Kuddle.Serialization;

namespace Kuddle.Tests.Serialization;

public class SerializerOptionsTests
{
    [Test]
    public async Task Serialize_IgnoreNullValues_WhenTrue_OmitsProperties()
    {
        var model = new OptionModel { Set = "value", NotSet = null };
        var options = new KdlSerializerOptions { IgnoreNullValues = true };

        var kdl = KdlSerializer.Serialize(model, options);

        await Assert.That(kdl).Contains("set=value");
        await Assert.That(kdl).DoesNotContain("not-set");
    }

    [Test]
    public async Task Serialize_IgnoreNullValues_WhenFalse_IncludesHashNull()
    {
        var model = new OptionModel { Set = "value", NotSet = null };
        var options = new KdlSerializerOptions { IgnoreNullValues = false };

        var kdl = KdlSerializer.Serialize(model, options);

        await Assert.That(kdl).Contains("set=value");
        await Assert.That(kdl).Contains("not-set=#null");
    }

    [Test]
    public async Task Serialize_SimpleCollectionNodeNames_WhenTrue_UsesDash()
    {
        var model = new { Items = new List<string> { "A", "B" } };
        var options = new KdlSerializerOptions { SimpleCollectionNodeNames = true };

        var kdl = KdlSerializer.Serialize(model, options);

        await Assert.That(kdl).Contains("- A");
        await Assert.That(kdl).Contains("- B");
    }

    [Test]
    public async Task Serialize_StringStyle_AllowBare_OmitsQuotesWhereSafe()
    {
        var model = new { Name = "SimpleName" };
        var options = new KdlSerializerOptions
        {
            Writer = KdlWriterOptions.Default with { StringStyle = KdlStringStyle.AllowBare },
        };

        var kdl = KdlSerializer.Serialize(model, options);

        await Assert.That(kdl).Contains("name=SimpleName");
        await Assert.That(kdl).DoesNotContain("\"SimpleName\"");
    }

    [Test]
    public async Task Deserialize_ValidateReservedTypes_WhenTrue_ThrowsOnOverflow()
    {
        var kdl = "node (u8)300";
        var options = new KdlSerializerOptions
        {
            Reader = KdlReaderOptions.Default with { ValidateReservedTypes = true },
        };

        await Assert
            .That(() => KdlSerializer.Deserialize<object>(kdl, options))
            .Throws<KuddleValidationException>();
    }

    [Test]
    public async Task Deserialize_ValidateReservedTypes_WhenFalse_AllowsInvalidValues()
    {
        var kdl = "node (u8)300";
        var options = new KdlSerializerOptions
        {
            Reader = KdlReaderOptions.Default with { ValidateReservedTypes = false },
        };

        var result = KdlSerializer.Deserialize<OptionModel>(kdl, options);
        await Assert.That(result).IsNotNull();
    }

    [Test]
    public async Task Serialize_RootMapping_AsDocument_PromotesProperties()
    {
        var model = new OptionModel { Set = "val1", Val2 = "val2" };
        var options = new KdlSerializerOptions { RootMapping = KdlRootMapping.AsDocument };
        string expected = """
set val1
val-2 val2

""".ReplaceLineEndings("\n");

        var kdl = KdlSerializer.Serialize(model, options);

        await Assert.That(kdl).IsEqualTo(expected);
        await Assert.That(kdl).DoesNotContain("option-model");
    }

    [Test]
    public async Task Serialize_IndentSize_IsRespected()
    {
        string expected = """
- {
  parent child=val
}

""".ReplaceLineEndings("\n");
        var model = new { Parent = new { Child = "val" } };
        var options = new KdlSerializerOptions
        {
            Writer = KdlWriterOptions.Default with { IndentSize = KdlWriterIndentSize.Two },
        };

        var kdl = KdlSerializer.Serialize(model, options);

        await Assert.That(kdl).IsEqualTo(expected);
    }

    [Test]
    public async Task Serialize_NestedAnonymousTypes_Fidelity()
    {
        string expected = """
- {
    level-1 level-2=Value
}

""".ReplaceLineEndings("\n");
        var model = new { Level1 = new { Level2 = "Value" } };

        var kdl = KdlSerializer.Serialize(model);

        await Assert.That(kdl).IsEqualTo(expected);
    }

    [Test]
    public async Task Serialize_PropagatesIndentType_Tabs()
    {
        string expected = """
- {
	parent child=val
}

""".ReplaceLineEndings("\n");
        var model = new { Parent = new { Child = "val" } };
        var options = new KdlSerializerOptions
        {
            Writer = KdlWriterOptions.Default with { IndentType = KdlWriterIndentType.Tabs },
        };

        var kdl = KdlSerializer.Serialize(model, options);

        await Assert.That(kdl).IsEqualTo(expected);
    }

    [Test]
    public async Task Serialize_PropagatesEscapeUnicode_True()
    {
        var model = new { Note = "©" };
        var options = new KdlSerializerOptions
        {
            Writer = KdlWriterOptions.Default with { EscapeUnicode = true },
        };

        var kdl = KdlSerializer.Serialize(model, options);

        await Assert.That(kdl).Contains(@"\u{00A9}");
        await Assert.That(kdl).DoesNotContain("©");
    }

    [Test]
    public async Task Serialize_PropagatesStringStyle_ViaShortcut()
    {
        string expected = """
"-" "key"="Value"

""".ReplaceLineEndings("\n");
        var model = new { Key = "Value" };

        var options = new KdlSerializerOptions { StringStyle = KdlStringStyle.AlwaysQuoted };

        var kdl = KdlSerializer.Serialize(model, options);

        await Assert.That(kdl).IsEqualTo(expected);
    }

    [Test]
    public async Task Deserialize_PropagatesValidateReservedTypes_ToReader()
    {
        var kdl = "node (u8)999";

        var options = new KdlSerializerOptions
        {
            Reader = KdlReaderOptions.Default with { ValidateReservedTypes = true },
        };

        await Assert
            .That(() => KdlSerializer.Deserialize<object>(kdl, options))
            .Throws<KuddleValidationException>();
    }

    [Test]
    public async Task DeserializeMany_PropagatesCancellationToken()
    {
        var kdl = "node1\nnode2\nnode3";
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var action = () =>
        {
            foreach (var item in KdlSerializer.DeserializeMany<object>(kdl, null, cts.Token)) { }
            return Task.CompletedTask;
        };

        await Assert.That(action).Throws<OperationCanceledException>();
    }

    [Test]
    public async Task Serializer_DoesNotPolluteDefaultOptions()
    {
        var customOptions = new KdlSerializerOptions { IgnoreNullValues = false };
        var model = new { Prop = (string?)null };

        var kdlCustom = KdlSerializer.Serialize(model, customOptions);
        await Assert.That(kdlCustom).Contains("#null");

        var kdlDefault = KdlSerializer.Serialize(model);
        await Assert.That(kdlDefault).DoesNotContain("#null");

        await Assert.That(KdlSerializerOptions.Default.IgnoreNullValues).IsTrue();
    }

    public class OptionModel
    {
        [KdlProperty]
        public string? Set { get; set; }

        [KdlProperty]
        public string? NotSet { get; set; }

        [KdlProperty]
        public string? Val2 { get; set; }
    }
}
