using System.Collections.Immutable;

namespace Kuddle.AST;

public record KdlTrivia(string Value, TriviaKind Kind) : KdlValue { }

public enum TriviaKind
{
    Unknown,
    WhiteSpace,
    NewLine,
    Comment,
}
