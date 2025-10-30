```text
document := bom? version? nodes

// Nodes
nodes := (line-space* node)* line-space*

base-node := slashdash? type? node-space* string
    (node-space+ slashdash? node-prop-or-arg)*
    // slashdashed node-children must always be after props and args.
    (node-space+ slashdash node-children)*
    (node-space+ node-children)?
    (node-space+ slashdash node-children)*
    node-space*
node := base-node node-terminator
final-node := base-node node-terminator?

// Entries
node-prop-or-arg := prop | value
node-children := '{' nodes final-node? '}'
node-terminator := single-line-comment | newline | ';' | eof

prop := string node-space* '=' node-space* value
value := type? node-space* (string | number | keyword)
type := '(' node-space* string node-space* ')'

// Strings
string := identifier-string | quoted-string | raw-string ¶

identifier-string := unambiguous-ident | signed-ident | dotted-ident
unambiguous-ident :=
    ((identifier-char - digit - sign - '.') identifier-char*)
    - disallowed-keyword-strings
signed-ident :=
    sign ((identifier-char - digit - '.') identifier-char*)?
dotted-ident :=
    sign? '.' ((identifier-char - digit) identifier-char*)?
identifier-char :=
    unicode - unicode-space - newline - [\\/(){};\[\]"#=]
    - disallowed-literal-code-points
disallowed-keyword-identifiers :=
    'true' | 'false' | 'null' | 'inf' | '-inf' | 'nan'

quoted-string :=
    '"' single-line-string-body '"' |
    '"""' newline
    (multi-line-string-body newline)?
    (unicode-space | ws-escape)* '"""'
single-line-string-body := (string-character - newline)*
multi-line-string-body := (('"' | '""')? string-character)*
string-character :=
    '\\' (["\\bfnrts] |
    'u{' hex-unicode '}') |
    ws-escape |
    [^\\"] - disallowed-literal-code-points
ws-escape := '\\' (unicode-space | newline)+
hex-digit := [0-9a-fA-F]
hex-unicode := hex-digit{1, 6} - surrogates
surrogates := [dD][8-9a-fA-F]hex-digit{2}
// U+D800-DFFF: D  8         00
//              D  F         FF

raw-string := '#' raw-string-quotes '#' | '#' raw-string '#'
raw-string-quotes :=
    '"' single-line-raw-string-body '"' |
    '"""' newline
    (multi-line-raw-string-body newline)?
    unicode-space* '"""'
single-line-raw-string-body :=
    '' |
    (single-line-raw-string-char - '"')
        single-line-raw-string-char*? |
    '"' (single-line-raw-string-char - '"')
        single-line-raw-string-char*?
single-line-raw-string-char :=
    unicode - newline - disallowed-literal-code-points
multi-line-raw-string-body :=
    (unicode - disallowed-literal-code-points)*?

// Numbers
number := keyword-number | hex | octal | binary | decimal

decimal := sign? integer ('.' integer)? exponent?
exponent := ('e' | 'E') sign? integer
integer := digit (digit | '_')*
digit := [0-9]
sign := '+' | '-'

hex := sign? '0x' hex-digit (hex-digit | '_')*
octal := sign? '0o' [0-7] [0-7_]*
binary := sign? '0b' ('0' | '1') ('0' | '1' | '_')*

// Keywords and booleans.
keyword := boolean | '#null'
keyword-number := '#inf' | '#-inf' | '#nan'
boolean := '#true' | '#false'

// Specific code points
bom := '\u{FEFF}'
disallowed-literal-code-points :=
    See Table (Disallowed Literal Code Points)
unicode := Any Unicode Scalar Value
unicode-space := See Table
    (All White_Space unicode characters which are not `newline`)

// Comments
single-line-comment := '//' ^newline* (newline | eof)
multi-line-comment := '/*' commented-block
commented-block :=
    '*/' | (multi-line-comment | '*' | '/' | [^*/]+) commented-block
slashdash := '/-' line-space*

// Whitespace
ws := unicode-space | multi-line-comment
escline := '\\' ws* (single-line-comment | newline | eof)
newline := See Table (All Newline White_Space)
// Whitespace where newlines are allowed.
line-space := node-space | newline | single-line-comment
// Whitespace within nodes,
// where newline-ish things must be esclined.
node-space := ws* escline ws* | ws+

// Version marker
version :=
    '/-' unicode-space* 'kdl-version' unicode-space+ ('1' | '2')
    unicode-space* newline
```
