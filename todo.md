Here is your updated roadmap.

You have effectively **completed Phase 1** (AST) and the heavy lifting of **Phase 2** (The Grammar/Parser Logic).

### Phase 1: The High-Fidelity DOM (AST) ✅

*Goal: Define immutable data structures that represent the physical text of a KDL file.*

* **1.1 Define the Base Object**
  * [x] Create `KdlObject` with Trivia support.
* **1.2 Implement Semantic Identifiers**
  * [x] Create `KdlIdentifier` (supports RawText and Type Annotations).
* **1.3 Implement Value Primitives**
  * [x] `KdlValue` base class.
  * [x] `KdlString` (Quoted, Raw, Multiline), `KdlBool`, `KdlNull`.
  * [x] `KdlNumber` (with lazy parsing logic).
* **1.4 Implement Node Structure**
  * [x] Create `KdlEntry` hierarchy (Argument, Property, SkippedEntry).
  * [x] Create `KdlNode` (Name, Entries List, Children Block).
* **1.5 Implement Containers**
  * [x] Create `KdlBlock` and `KdlDocument`.

### Phase 2: The Parser Integration 🚧

*Goal: Build the engine that populates the AST while enforcing the KDL spec.*

* **2.1 Tokenizer & Grammar**
  * [x] Implement `KuddleGrammar` using Parlot.
  * [x] Implement complex String parsing (Raw, Multiline, Escapes).
  * [x] Implement Number parsing (Hex, Binary, Octal).
* **2.2 Structural Parsing**
  * [x] Implement Node parsing (Name, Type, properties).
  * [x] Implement Recursion (Node -> Children -> Nodes).
  * [x] Implement "Slash-Dash" `/-` logic for skipping nodes/entries/children.
* **2.3 API Wrapper**
  * [ ] Create the public static `KdlParser.Parse(string input)` method that invokes `KuddleGrammar.Document.Parse(...)`.
  * [ ] Add `KdlParseException` wrapping Parlot errors with line/column info.
* **2.4 Reserved Type Validation**
  * [ ] **(New)** Add a post-parse visitor or validation pass to ensure `(u8)` values fit in bytes, `(uuid)` are valid GUIDs, etc.

### Phase 3: The Developer Experience (DX) & Extensions

*Goal: Provide a usable API without polluting the pure AST.*

* **3.1 Value Extensions (The "TryGet" Pattern)**
  * [ ] Create `KdlValueExtensions`.
  * [ ] Implement `TryGetUuid`, `TryGetDateTime`, `TryGetInt`, etc.
* **3.2 Mutation Factories**
  * [ ] Add `KdlValue.From(Guid)`, `KdlValue.From(int)` helpers.
* **3.3 Navigation Helpers**
  * [ ] Add indexers: `node["propName"]`.
  * [ ] Add `node.Arguments` (computed view).

### Phase 4: Serialization

*Goal: Output KDL from the AST.*

* **4.1 Round-Trip Writer**
  * [ ] Create `KdlWriter` implementation.
  * [ ] Logic: Iterate AST, write `LeadingTrivia`, `TypeAnnotation`, `RawText`, `TrailingTrivia`.
* **4.2 Formatting Writer**
  * [ ] Create `KdlFormatter` (or options for `KdlWriter`).
  * [ ] Logic to ignore stored trivia and re-indent based on settings.

### Phase 5: Verification 🚧

*Goal: Ensure correctness.*

* **5.1 Unit Tests**
  * [ ] **(In Progress)** AST Structural Tests.
  * [ ] Grammar Logic Tests (Strings, Numbers).
* **5.2 Integration Tests**
  * [ ] Run against the official `kdl-org/kdl` test suite.

### Phase 6: Object Mapping (POCOs) & Source Gen

*Goal: Map AST to strong C# types.*

* **6.1 The "Sidecar" Source Generator**
  * [ ] Create `[KdlAnnotate]` attribute.
  * [ ] Generate partial classes for metadata storage.
* **6.2 The Mapper**
  * [ ] Implement `KdlSerializer.Deserialize<T>()`.
  * [ ] Map Nodes to Classes, Entries to Properties.
* **6.3 Polymorphism**
  * [ ] Implement Discriminator logic using Node Annotations.
