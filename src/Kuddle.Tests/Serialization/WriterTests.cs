using Kuddle.AST;
using Kuddle.Parser;
using Kuddle.Serialization;

namespace Kuddle.Tests.Serialization;

public class WriterTests
{
    [Test]
    public async Task Write_SimpleNode_FormatsCorrectly()
    {
        var kdl = "node 1 2 key=\"val\"";
        var doc = KdlParser.Parse(kdl);

        var output = KdlWriter.Write(doc, KdlWriterOptions.Default);

        // Output should be normalized (e.g., quotes added to value if needed)
        await Assert.That(output.Trim()).IsEqualTo("node 1 2 key=\"val\"");
    }

    [Test]
    public async Task Write_NestedStructure_IndentsCorrectly()
    {
        var kdl = "parent { child; }";
        var doc = KdlParser.Parse(kdl);

        var output = KdlWriter.Write(doc);

        var expected =
            @"parent {
    child
}
";
        // Normalize newlines for test stability
        await Assert.That(output.Replace("\r\n", "\n")).IsEqualTo(expected.Replace("\r\n", "\n"));
    }

    [Test]
    public async Task Write_ComplexString_EscapesCorrectly()
    {
        var kdl = "node \"line1\\nline2\"";
        var doc = KdlParser.Parse(kdl);

        var output = KdlWriter.Write(doc);

        // Writer should re-escape the newline
        await Assert.That(output.Trim()).IsEqualTo("node \"line1\\nline2\"");
    }

    [Test]
    public async Task Write_BareIdentifier_QuotesIfInvalid()
    {
        // "key=value" is one arg containing '='? No, standard parser splits it.
        // Let's force a string with special chars
        var doc = new KdlDocument
        {
            Nodes =
            [
                new KdlNode(new KdlString("node name", StringKind.Quoted)), // Space in name
            ],
        };

        var output = KdlWriter.Write(doc);

        // Should force quotes because of space
        await Assert.That(output.Trim()).IsEqualTo("\"node name\"");
    }
}
