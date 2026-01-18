using Kuddle.AST;
using Kuddle.Parser;
using Kuddle.Serialization;

namespace Kuddle.Tests.Formatting;

/// <summary>
/// Tests for KdlWriter formatting, escaping, and output options.
/// </summary>
public class KdlWriterTests
{
    #region Basic Formatting Tests

    [Test]
    public async Task Write_SimpleNode_FormatsCorrectly()
    {
        var kdl = "node 1 2 key=\"val\"";
        var doc = KdlReader.Read(kdl);

        var output = KdlWriter.Write(
            doc,
            new KdlWriterOptions { StringStyle = KdlStringStyle.Preserve }
        );

        await Assert.That(output.Trim()).IsEqualTo("node 1 2 key=\"val\"");
    }

    [Test]
    public async Task Write_NestedStructure_IndentsCorrectly()
    {
        var kdl = "parent { child; }";
        var doc = KdlReader.Read(kdl);

        var output = KdlWriter.Write(
            doc,
            new KdlWriterOptions { StringStyle = KdlStringStyle.Preserve }
        );

        var expected = @"parent {
    child;
}
".Replace("\r\n", "\n");
        await Assert.That(output).IsEqualTo(expected);
    }

    [Test]
    public async Task Write_DeeplyNestedStructure_IndentsEachLevel()
    {
        var kdl = """
            root {
                level1 {
                    level2 {
                        leaf
                    }
                }
            }
            """;
        var doc = KdlReader.Read(kdl);

        var output = KdlWriter.Write(doc);

        await Assert.That(output).Contains("        leaf");
    }

    [Test]
    public async Task Write_EmptyDocument_ReturnsEmptyString()
    {
        var doc = new KdlDocument();

        var output = KdlWriter.Write(doc);

        await Assert.That(output).IsEqualTo("");
    }

    #endregion

    #region String Escaping Tests

    [Test]
    public async Task Write_ComplexString_EscapesCorrectly()
    {
        var kdl = "node \"line1\\nline2\"";
        var doc = KdlReader.Read(kdl);

        var output = KdlWriter.Write(
            doc,
            new KdlWriterOptions { StringStyle = KdlStringStyle.Preserve }
        );

        await Assert.That(output.Trim()).IsEqualTo("node \"line1\\nline2\"");
    }

    [Test]
    public async Task Write_BareIdentifier_QuotesIfInvalid()
    {
        var doc = new KdlDocument
        {
            Nodes = [new KdlNode(new KdlString("node name", StringKind.Quoted))],
        };

        var output = KdlWriter.Write(
            doc,
            new KdlWriterOptions { StringStyle = KdlStringStyle.Preserve }
        );

        await Assert.That(output.Trim()).IsEqualTo("\"node name\"");
    }

    [Test]
    public async Task Write_StringWithTab_EscapesTab()
    {
        var kdl = "node \"hello\\tworld\"";
        var doc = KdlReader.Read(kdl);

        var output = KdlWriter.Write(
            doc,
            new KdlWriterOptions { StringStyle = KdlStringStyle.Preserve }
        );

        await Assert.That(output).Contains("\\t");
    }

    [Test]
    public async Task Write_StringWithBackslash_EscapesBackslash()
    {
        var kdl = "node \"path\\\\to\\\\file\"";
        var doc = KdlReader.Read(kdl);

        var output = KdlWriter.Write(
            doc,
            new KdlWriterOptions { StringStyle = KdlStringStyle.Preserve }
        );

        await Assert.That(output).Contains("\\\\");
    }

    [Test]
    public async Task Write_StringWithQuotes_EscapesQuotes()
    {
        var kdl = "node \"say \\\"hello\\\"\"";
        var doc = KdlReader.Read(kdl);

        var output = KdlWriter.Write(
            doc,
            new KdlWriterOptions { StringStyle = KdlStringStyle.Preserve }
        );

        await Assert.That(output).Contains("\\\"");
    }

    #endregion

    #region StringKind Tests

    [Test]
    public async Task Write_BareString_OutputsWithoutQuotes()
    {
        var doc = new KdlDocument
        {
            Nodes =
            [
                new KdlNode(new KdlString("node", StringKind.Bare))
                {
                    Entries = [new KdlArgument(new KdlString("barevalue", StringKind.Bare))],
                },
            ],
        };

        var output = KdlWriter.Write(doc);

        await Assert.That(output.Trim()).IsEqualTo("node barevalue");
    }

    [Test]
    public async Task Write_QuotedString_OutputsWithQuotes()
    {
        var doc = new KdlDocument
        {
            Nodes =
            [
                new KdlNode(new KdlString("node", StringKind.Bare))
                {
                    Entries = [new KdlArgument(new KdlString("quoted value", StringKind.Quoted))],
                },
            ],
        };

        var output = KdlWriter.Write(doc);

        await Assert.That(output).Contains("\"quoted value\"");
    }

    #endregion

    #region Number Output Tests

    [Test]
    public async Task Write_DecimalNumber_OutputsDecimal()
    {
        var doc = new KdlDocument
        {
            Nodes =
            [
                new KdlNode(new KdlString("node", StringKind.Bare))
                {
                    Entries = [new KdlArgument(new KdlNumber("42"))],
                },
            ],
        };

        var output = KdlWriter.Write(doc);

        await Assert.That(output.Trim()).IsEqualTo("node 42");
    }

    [Test]
    public async Task Write_NegativeNumber_OutputsWithSign()
    {
        var doc = new KdlDocument
        {
            Nodes =
            [
                new KdlNode(new KdlString("node", StringKind.Bare))
                {
                    Entries = [new KdlArgument(new KdlNumber("-123"))],
                },
            ],
        };

        var output = KdlWriter.Write(doc);

        await Assert.That(output.Trim()).IsEqualTo("node -123");
    }

    [Test]
    public async Task Write_FloatNumber_OutputsDecimalPoint()
    {
        var doc = new KdlDocument
        {
            Nodes =
            [
                new KdlNode(new KdlString("node", StringKind.Bare))
                {
                    Entries = [new KdlArgument(new KdlNumber("3.14"))],
                },
            ],
        };

        var output = KdlWriter.Write(doc);

        await Assert.That(output.Trim()).IsEqualTo("node 3.14");
    }

    #endregion

    #region Boolean and Null Output Tests

    [Test]
    public async Task Write_BoolTrue_OutputsHashTrue()
    {
        var doc = new KdlDocument
        {
            Nodes =
            [
                new KdlNode(new KdlString("node", StringKind.Bare))
                {
                    Entries = [new KdlArgument(new KdlBool(true))],
                },
            ],
        };

        var output = KdlWriter.Write(doc);

        await Assert.That(output.Trim()).IsEqualTo("node #true");
    }

    [Test]
    public async Task Write_BoolFalse_OutputsHashFalse()
    {
        var doc = new KdlDocument
        {
            Nodes =
            [
                new KdlNode(new KdlString("node", StringKind.Bare))
                {
                    Entries = [new KdlArgument(new KdlBool(false))],
                },
            ],
        };

        var output = KdlWriter.Write(doc);

        await Assert.That(output.Trim()).IsEqualTo("node #false");
    }

    [Test]
    public async Task Write_Null_OutputsHashNull()
    {
        var doc = new KdlDocument
        {
            Nodes =
            [
                new KdlNode(new KdlString("node", StringKind.Bare))
                {
                    Entries = [new KdlArgument(new KdlNull())],
                },
            ],
        };

        var output = KdlWriter.Write(doc);

        await Assert.That(output.Trim()).IsEqualTo("node #null");
    }

    #endregion

    #region Type Annotation Output Tests

    [Test]
    public async Task Write_TypeAnnotationOnNode_OutputsInParentheses()
    {
        var kdl = "(mytype)node 123";
        var doc = KdlReader.Read(kdl);

        var output = KdlWriter.Write(doc);

        await Assert.That(output).Contains("(mytype)node");
    }

    [Test]
    public async Task Write_TypeAnnotationOnValue_OutputsBeforeValue()
    {
        var uuid = Guid.NewGuid().ToString();
        var kdl = $"node (uuid)\"{uuid}\"";
        var doc = KdlReader.Read(kdl);

        var output = KdlWriter.Write(doc);

        await Assert.That(output).Contains("(uuid)");
    }

    #endregion

    #region Property Output Tests

    [Test]
    public async Task Write_Property_OutputsKeyEqualsValue()
    {
        var doc = new KdlDocument
        {
            Nodes =
            [
                new KdlNode(new KdlString("node", StringKind.Bare))
                {
                    Entries =
                    [
                        new KdlProperty(
                            new KdlString("key", StringKind.Bare),
                            new KdlString("hello world", StringKind.Quoted)
                        ),
                    ],
                },
            ],
        };

        var output = KdlWriter.Write(doc);

        await Assert.That(output).Contains("key=\"hello world\"");
    }

    [Test]
    public async Task Write_PropertyWithNumericValue_OutputsCorrectly()
    {
        var doc = new KdlDocument
        {
            Nodes =
            [
                new KdlNode(new KdlString("node", StringKind.Bare))
                {
                    Entries =
                    [
                        new KdlProperty(
                            new KdlString("count", StringKind.Bare),
                            new KdlNumber("42")
                        ),
                    ],
                },
            ],
        };

        var output = KdlWriter.Write(doc);

        await Assert.That(output).Contains("count=42");
    }

    #endregion
}
