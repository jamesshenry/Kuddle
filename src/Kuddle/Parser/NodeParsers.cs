// using Parlot;
// using Parlot.Fluent;
// using static Parlot.Fluent.Parsers;

// namespace Kuddle.Parser;

// public static class NodeParsers
// {
//     static NodeParsers()
//     {
//         // type := '(' node-space* string node-space* ')'
//         Type = Between(
//             Literals.Char('(').And(ZeroOrMany(KuddleGrammar.NodeSpace)),
//             KuddleGrammar.String,
//             ZeroOrMany(KuddleGrammar.NodeSpace).And(Literals.Char(')'))
//         );
//         // value := type? node-space* (string | number | keyword)
//         Value = KuddleGrammar
//             .Type.Optional()
//             .SkipAnd(KuddleGrammar.NodeSpace.ZeroOrMany())
//             .SkipAnd(
//                 OneOf(KuddleGrammar.Number, KuddleGrammar.String, Capture(KuddleGrammar.Keyword))
//             );

//         // prop := string node-space* '=' node-space* value
//         Prop = Capture(
//             KuddleGrammar
//                 .String.AndSkip(KuddleGrammar.NodeSpace.ZeroOrMany())
//                 .SkipAnd(Literals.Char('='))
//                 .AndSkip(KuddleGrammar.NodeSpace.ZeroOrMany())
//                 .And(Value)
//         );

//         // node-prop-or-arg := prop | value
//         NodePropOrArg = OneOf<TextSpan>(Prop, Value);

//         // node-terminator := single-line-comment | newline | ';' | eof
//         NodeTerminator = OneOf<TextSpan>(
//             KuddleGrammar.SingleLineComment,
//             KuddleGrammar.SingleNewLine,
//             Literals.Char(';').Then(c => new TextSpan(c.ToString())),
//             Always().Eof().Then(_ => new TextSpan(""))
//         );

//         // For now, stub the complex parsers
//         NodeChildren = Always<TextSpan>();
//         BaseNode = KuddleGrammar.String;
//         Node = KuddleGrammar.String.AndSkip(Literals.Char(';'));
//         FinalNode = KuddleGrammar.String;
//         Nodes = KuddleGrammar.String;
//     }

//     public static readonly Parser<TextSpan> Type = KuddleGrammar.Type;
//     public static readonly Parser<TextSpan> Value;
//     public static readonly Parser<TextSpan> Prop;
//     public static readonly Parser<TextSpan> NodePropOrArg;
//     public static readonly Parser<TextSpan> NodeChildren;
//     public static readonly Parser<TextSpan> BaseNode;
//     public static readonly Parser<TextSpan> NodeTerminator;
//     public static readonly Parser<TextSpan> Node;
//     public static readonly Parser<TextSpan> FinalNode;
//     public static readonly Parser<TextSpan> Nodes;
// }
