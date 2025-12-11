using Kuddle.AST;
using Kuddle.Parser;
using Kuddle.Serialization;

namespace Kuddle.Tests.Serialization;

public class KdlWriterTests
{
    [Test]
    public async Task Write_SimpleNode_FormatsCorrectly()
    {
        var kdl = "node 1 2 key=\"val\"";
        var doc = KdlReader.Parse(kdl);

        var output = KdlWriter.Write(doc, KdlWriterOptions.Default);

        await Assert.That(output.Trim()).IsEqualTo("node 1 2 key=\"val\"");
    }

    [Test]
    public async Task Write_NestedStructure_IndentsCorrectly()
    {
        var kdl = "parent { child; }";
        var doc = KdlReader.Parse(kdl);

        var output = KdlWriter.Write(doc);

        var expected = @"parent {
    child;
}
".Replace("\r\n", "\n");
        await Assert.That(output).IsEqualTo(expected);
    }

    [Test]
    public async Task Write_ComplexString_EscapesCorrectly()
    {
        var kdl = "node \"line1\\nline2\"";
        var doc = KdlReader.Parse(kdl);

        var output = KdlWriter.Write(doc);

        await Assert.That(output.Trim()).IsEqualTo("node \"line1\\nline2\"");
    }

    [Test]
    public async Task Write_BareIdentifier_QuotesIfInvalid()
    {
        var doc = new KdlDocument
        {
            Nodes = [new KdlNode(new KdlString("node name", StringKind.Quoted))],
        };

        var output = KdlWriter.Write(doc);

        await Assert.That(output.Trim()).IsEqualTo("\"node name\"");
    }
}
