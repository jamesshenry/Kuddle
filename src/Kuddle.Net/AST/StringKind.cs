using System;

namespace Kuddle.AST;

[Flags]
public enum StringKind
{
    Bare = 1,
    Quoted = 2,
    Raw = 4,
    MultiLine = 8,

    MultiLineRaw = MultiLine | Raw,
    QuotedRaw = Quoted | Raw,
}
