// using Kuddle.Parser;

// namespace Kuddle.Tests;

// public class StringParsersTests
// {
//     // Note: These tests are stubs until StringParsers is implemented
//     // They represent the string parsing rules from the KDL grammar:
//     // string := identifier-string | quoted-string | raw-string
//     // identifier-string := unambiguous-ident | signed-ident | dotted-ident
//     // quoted-string := '"' single-line-string-body '"' | '"""' newline (multi-line-string-body newline)? (unicode-space | ws-escape)* '"""'
//     // raw-string := '#' raw-string-quotes '#' | '#' raw-string '#'

//     [Test]
//     public async Task IdentifierString_ParsesUnambiguousIdentifier()
//     {
//         var sut = StringParsers.IdentifierString;

//         var input = "hello";
//         bool success = sut.TryParse(input, out var value);

//         await Assert.That(success).IsTrue();
//         await Assert.That(value.ToString()).IsEqualTo(input);
//     }

//     [Test]
//     public async Task IdentifierString_ParsesIdentifierWithNumbers()
//     {
//         var sut = StringParsers.IdentifierString;

//         var input = "world123";
//         bool success = sut.TryParse(input, out var value);

//         await Assert.That(success).IsTrue();
//         await Assert.That(value.ToString()).IsEqualTo(input);
//     }

//     [Test]
//     public async Task IdentifierString_ParsesIdentifierWithUnderscore()
//     {
//         var sut = StringParsers.IdentifierString;

//         var input = "test_case";
//         bool success = sut.TryParse(input, out var value);

//         await Assert.That(success).IsTrue();
//         await Assert.That(value.ToString()).IsEqualTo(input);
//     }

//     [Test]
//     public async Task IdentifierString_ParsesSignedIdentifier()
//     {
//         var sut = StringParsers.IdentifierString;

//         var input = "+positive";
//         bool success = sut.TryParse(input, out var value);

//         await Assert.That(success).IsTrue();
//         await Assert.That(value.ToString()).IsEqualTo(input);
//     }

//     [Test]
//     public async Task IdentifierString_ParsesNegativeSignedIdentifier()
//     {
//         var sut = StringParsers.IdentifierString;

//         var input = "-negative";
//         bool success = sut.TryParse(input, out var value);

//         await Assert.That(success).IsTrue();
//         await Assert.That(value.ToString()).IsEqualTo(input);
//     }

//     [Test]
//     public async Task IdentifierString_ParsesDottedIdentifier()
//     {
//         var sut = StringParsers.IdentifierString;

//         var input = ".hidden";
//         bool success = sut.TryParse(input, out var value);

//         await Assert.That(success).IsTrue();
//         await Assert.That(value.ToString()).IsEqualTo(input);
//     }

//     [Test]
//     public async Task IdentifierString_ParsesSignedDottedIdentifier()
//     {
//         var sut = StringParsers.IdentifierString;

//         var input = "+.positive";
//         bool success = sut.TryParse(input, out var value);

//         await Assert.That(success).IsTrue();
//         await Assert.That(value.ToString()).IsEqualTo(input);
//     }

//     [Test]
//     public async Task IdentifierString_RejectsTrueKeyword()
//     {
//         var sut = StringParsers.IdentifierString;

//         var input = "true";
//         bool success = sut.TryParse(input, out var value);

//         await Assert.That(success).IsFalse();
//     }

//     [Test]
//     public async Task IdentifierString_RejectsFalseKeyword()
//     {
//         var sut = StringParsers.IdentifierString;

//         var input = "false";
//         bool success = sut.TryParse(input, out var value);

//         await Assert.That(success).IsFalse();
//     }

//     [Test]
//     public async Task IdentifierString_RejectsNullKeyword()
//     {
//         var sut = StringParsers.IdentifierString;

//         var input = "null";
//         bool success = sut.TryParse(input, out var value);

//         await Assert.That(success).IsFalse();
//     }

//     [Test]
//     public async Task IdentifierString_RejectsInfKeyword()
//     {
//         var sut = StringParsers.IdentifierString;

//         var input = "inf";
//         bool success = sut.TryParse(input, out var value);

//         await Assert.That(success).IsFalse();
//     }

//     [Test]
//     public async Task IdentifierString_RejectsNegativeInfKeyword()
//     {
//         var sut = StringParsers.IdentifierString;

//         var input = "-inf";
//         bool success = sut.TryParse(input, out var value);

//         await Assert.That(success).IsFalse();
//     }

//     [Test]
//     public async Task IdentifierString_RejectsNanKeyword()
//     {
//         var sut = StringParsers.IdentifierString;

//         var input = "nan";
//         bool success = sut.TryParse(input, out var value);

//         await Assert.That(success).IsFalse();
//     }

//     [Test]
//     public async Task QuotedString_ParsesSingleLineString()
//     {
//         var sut = StringParsers.QuotedString;

//         var input = "\"hello world\"";
//         bool success = sut.TryParse(input, out var value);

//         await Assert.That(success).IsTrue();
//         await Assert.That(value.ToString()).IsEqualTo(input);
//     }

//     [Test]
//     public async Task QuotedString_ParsesEmptyString()
//     {
//         var sut = StringParsers.QuotedString;

//         var input = "\"\"";
//         bool success = sut.TryParse(input, out var value);

//         await Assert.That(success).IsTrue();
//         await Assert.That(value.ToString()).IsEqualTo(input);
//     }

//     [Test]
//     public async Task QuotedString_HandlesEscapeSequences()
//     {
//         var sut = StringParsers.QuotedString;

//         var input = "\"\\n\\t\\\\\\\"\"";
//         bool success = sut.TryParse(input, out var value);

//         await Assert.That(success).IsTrue();
//         await Assert.That(value.ToString()).IsEqualTo(input);
//     }

//     [Test]
//     public async Task QuotedString_HandlesUnicodeEscapes()
//     {
//         var sut = StringParsers.QuotedString;

//         var input = "\"\\u{1F600}\"";
//         bool success = sut.TryParse(input, out var value);

//         await Assert.That(success).IsTrue();
//         await Assert.That(value.ToString()).IsEqualTo(input);
//     }

//     [Test]
//     public async Task QuotedString_HandlesWhitespaceEscapes()
//     {
//         var sut = StringParsers.QuotedString;

//         var input = "\"hello \\\nworld\"";
//         bool success = sut.TryParse(input, out var value);

//         await Assert.That(success).IsTrue();
//         await Assert.That(value.ToString()).IsEqualTo(input);
//     }

//     [Test]
//     public async Task RawString_ParsesSimpleRawString()
//     {
//         var sut = StringParsers.RawString;

//         var input = "#\"hello world\"#";
//         bool success = sut.TryParse(input, out var value);

//         await Assert.That(success).IsTrue();
//         await Assert.That(value.ToString()).IsEqualTo(input);
//     }

//     [Test]
//     public async Task RawString_ParsesRawStringWithQuotes()
//     {
//         var sut = StringParsers.RawString;

//         var input = "#\"content with \"quotes\"\"#";
//         bool success = sut.TryParse(input, out var value);

//         await Assert.That(success).IsTrue();
//         await Assert.That(value.ToString()).IsEqualTo(input);
//     }

//     [Test]
//     public async Task RawString_HandlesMultipleHashDelimiters()
//     {
//         var sut = StringParsers.RawString;

//         var input = "##\"content with \" quotes\"##";
//         bool success = sut.TryParse(input, out var value);

//         await Assert.That(success).IsTrue();
//         await Assert.That(value.ToString()).IsEqualTo(input);
//     }

//     [Test]
//     public async Task String_ParsesIdentifierString()
//     {
//         var sut = StringParsers.String;

//         var input = "hello";
//         bool success = sut.TryParse(input, out var value);

//         await Assert.That(success).IsTrue();
//         await Assert.That(value.ToString()).IsEqualTo(input);
//     }

//     [Test]
//     public async Task String_ParsesQuotedString()
//     {
//         var sut = StringParsers.String;

//         var input = "\"hello world\"";
//         bool success = sut.TryParse(input, out var value);

//         await Assert.That(success).IsTrue();
//         await Assert.That(value.ToString()).IsEqualTo(input);
//     }

//     [Test]
//     public async Task String_ParsesRawString()
//     {
//         var sut = StringParsers.String;

//         var input = "#\"hello world\"#";
//         bool success = sut.TryParse(input, out var value);

//         await Assert.That(success).IsTrue();
//         await Assert.That(value.ToString()).IsEqualTo(input);
//     }

//     [Test]
//     public async Task QuotedString_RejectsUnterminatedString()
//     {
//         var sut = StringParsers.QuotedString;

//         var input = "\"unterminated string";
//         bool success = sut.TryParse(input, out var value);

//         await Assert.That(success).IsFalse();
//     }

//     [Test]
//     public async Task RawString_RejectsMismatchedDelimiters()
//     {
//         var sut = StringParsers.RawString;

//         var input = "#\"content\"##";
//         bool success = sut.TryParse(input, out var value);

//         await Assert.That(success).IsFalse();
//     }
// }
