### Phase 1: The High-Fidelity DOM (AST)

*Goal: Define immutable data structures that can represent every byte of a KDL file, including comments and formatting.*

* **1.1 Define the "Trivia" Concept** (Crucial for round-tripping)
  * [ ] Create a `KdlTrivia` record (holds whitespace, newlines, comments).
  * [ ] Decide on storage: Usually, every Node has `LeadingTrivia` (before the node) and `TrailingTrivia` (after the node, usually the newline).
* **1.2 Implement Value Primitives (The `Kuddle.AST` namespace)**
  * [ ] `KdlValue` (Abstract Base)
  * [ ] `KdlString` (Raw, Quoted, Block strings) - Store the *raw representation* (including quotes) to preserve style.
  * [ ] `KdlNumber` (Your existing implementation: lazy parsing, store raw string).
  * [ ] `KdlBool`, `KdlNull`.
* **1.3 Implement Structural Nodes**
  * [ ] `KdlNode`: Needs `Name`, `Arguments` (List), `Properties` (Map/List), `Children` (Block), plus Trivia.
  * [ ] `KdlDocument`: The root container. Holds a list of Nodes and global Trivia (e.g., file header comments).
* **1.4 Implement "Property" Structures**
  * [ ] Unlike JSON, KDL properties are ordered. Decide if you use `List<KeyValuePair<string, KdlValue>>` or a custom `KdlProperty` record to hold the `=` sign trivia (e.g., spaces around the equals sign).

### Phase 2: The Parser Integration (Lexer & Parser)

*Goal: Connect your existing grammar logic to the new AST records.*

* **2.1 Update Tokenizer for "Trivia"**
  * [ ] **Stop skipping whitespace.** Most parsers `skip` spaces. You must `consume` them and attach them to the current or next token.
  * [ ] Capture comments (`//`, `/* */`, `/-`) as tokens/trivia, not ignored regions.
* **2.2 Implement the Builder/Parser**
  * [ ] Create `KdlParser.Parse(string/Span)` returning `KdlDocument`.
  * [ ] Ensure the parser constructs the AST records *with* the captured Trivia.
* **2.3 Error Handling**
  * [ ] Implement specific exceptions (`KdlParseException`) with line/column info.
  * [ ] (Optional/Advanced) Error recovery: Can you return a partial AST even if a semicolon is missing? (Roslyn does this, but hard for v1).

### Phase 3: The Query & Manipulation API (DX Layer)

*Goal: Make the library usable for developers (avoiding the Kadlet traps).*

* **3.1 Navigation Helpers**
  * [ ] Implement indexers: `node["childName"]`.
  * [ ] Implement `GetArg<T>(index)` and `GetProp<T>(key)` utilizing your lazy number parsing.
* **3.2 Modification (Immutability)**
  * [ ] Verify `with` expressions work cleanly: `node with { Name = "new_name" }`.
  * [ ] (Optional) Create "Fluent" helper extensions if `with` syntax proves too verbose for deep updates.
* **3.3 LINQ Support**
  * [ ] Ensure `Children` implements `IEnumerable<KdlNode>` so users can use `.Where()`, `.Select()`.

### Phase 4: Serialization (The Output)

*Goal: Output KDL. This needs two modes: "Round-Trip" and "Formatted".*

* **4.1 The Visitor Interface**
  * [ ] Define `IKdlVisitor` interface.
  * [ ] Implement `Accept` methods on all AST records.
* **4.2 The Round-Trip Writer**
  * [ ] Create `KdlWriter` implementation.
  * [ ] Logic: Iterate the AST. If `Trivia` exists (e.g., `SourceValue` is present), write it exactly.
  * *Result:* `Parse("node   1").ToString()` returns `"node   1"`.
* **4.3 The Pretty-Print Formatter**
  * [ ] Create `KdlFormatter` (or a specific configuration for `KdlWriter`).
  * [ ] Logic: Ignore stored Trivia. Use configured indentation (tabs/spaces) and spacing rules.
  * *Result:* `Parse("node   1").ToPrettyString()` returns `"node 1"`.

### Phase 5: Verification & Polish

*Goal: Gold Standard Certification.*

* **5.1 Test Suite Integration**
  * [ ] Clone the `kdl-org/kdl` repository.
  * [ ] Implement a test runner that executes their standard test suite against your parser.
* **5.2 Fuzzing**
  * [ ] Throw garbage data at it. Ensure it throws handled Exceptions, not `NullReferenceException` or `IndexOutOfRangeException`.
* **5.3 Benchmarking**
  * [ ] Use `BenchmarkDotNet`. Compare parsing speed and memory allocation against `Kadlet` (and maybe `Newtonsoft` just for a baseline comparison).

---

### Technical Detail: The "Trivia" Architecture

To save you time in **Phase 1.1**, here is the recommended "Roslyn-lite" structure for your records to support round-tripping:

```csharp
public abstract record KdlSyntaxElement 
{
    // Trivia: Comments, spaces, newlines that appear BEFORE this element
    public string LeadingTrivia { get; init; } = "";
    
    // Trivia: Comments, spaces that appear AFTER this element (rarely used, but good for safety)
    public string TrailingTrivia { get; init; } = "";
}

public sealed record KdlNode(string Name) : KdlSyntaxElement
{
    // The raw string of the identifier if it was quoted (e.g. "foo" vs foo)
    public string RawName { get; init; } = Name; 
    
    // ... Children, Props, Args
}
```

**Recommendation:** Start **Phase 1** immediately. Without the Trivia-aware AST defined, you cannot finalize your parser logic.
