# TODO

## Phase 1: Foundation & Metadata (The "Brain")

*Before parsing a single byte, your system must understand the shape of your C# types.*

* **1.1. Define the Attribute Suite**
  * [ ] Create `KdlPropertyDictionaryAttribute`.
  * [ ] Create `KdlNodeDictionaryAttribute` (with `string NodeName` property).
  * [ ] Create `KdlKeyedNodesAttribute` (with `string NodeName` and `string KeyProperty`).
  * [ ] Add `Enforce` bool to existing attributes (for future Strict Mode).

* **1.2. Upgrade `KdlEntryMapping`**
  * [ ] Update the record to detect the 3 new Dictionary attributes.
  * [ ] Add logic to resolve the effective `Name` (handling the fallback to property names).
  * [ ] Add a field to store `KeyPropertyName` (specifically for `KdlKeyedNodes`).

* **1.3. Implement Type Inspection Utilities**
  * [ ] Implement `TypeHelpers.GetDictionaryInfo(Type t)`: Returns `(bool IsDict, Type KeyType, Type ValueType)`. Must handle `class MyDict : Dictionary<string, int>`.
  * [ ] Implement `TypeHelpers.GetCollectionInfo(Type t)`: Returns `(bool IsCol, Type ElemType)`. Must handle Arrays and Lists.

* **1.4. Build the `TypeMetadata` Validator**
  * [ ] Implement **Attribute Exclusion Check**: Throw if a property has both `[KdlProperty]` and `[KdlNode]`.
  * [ ] Implement **Contiguity Check**: Throw if `[KdlArgument]` indices have gaps (e.g., 0, 2).
  * [ ] Implement **Bucketing**: Pre-sort mappings into `Arguments`, `Properties`, `Children`, and `Dictionaries` lists so the parser doesn't scan attributes at runtime.

---

## Phase 2: Core Logic Refactoring (The "Muscle")

*Update the main loop to use your new Metadata instead of raw reflection.*

* **2.1. Refactor `Deserialize<T>`**
  * [ ] Change the entry point to look up `TypeMetadata.For<T>()`.
  * [ ] Replace current attribute lookups with loops over the pre-calculated Metadata buckets.

* **2.2. Implement Strict Argument Mapping**
  * [ ] Iterate `meta.ArgumentAttributes`.
  * [ ] Map KDL Argument `i` to Property `i`.
  * [ ] **Validation**: If KDL has fewer arguments than required (non-nullable) properties, decide if you throw or use default.

* **2.3. Implement Child Node Mapping (`[KdlNode]`)**
  * [ ] **Collection Mode**: If `meta.IsCollection` or property is `List<T>`, find *all* matching children, deserialize, and Add.
  * [ ] **Single Object Mode**: If property is a class, find *exactly one* matching child. Throw if multiple exist (ambiguous match).
  * [ ] **Scalar Flattening**: If property is `int`/`string`, find child, read `Arg[0]`, assign.

---

## Phase 3: The Dictionary Engine (The "Complex Part")

*Implement the three strategies for IDictionary.*

* **3.1. Implement `[KdlPropertyDictionary]`**
  * [ ] Iterate over **Properties** of the *current* KDL node.
  * [ ] Filter out properties already mapped to explicit C# properties.
  * [ ] Cast/Convert remaining values to `TValue` (usually string) and add to the dictionary.

* **3.2. Implement `[KdlNodeDictionary]`**
  * [ ] Iterate over **Child Nodes** of the current KDL node.
  * [ ] **Key Extraction**: Use the Child Node's Name.
  * [ ] **Value Extraction (Scalar)**: If `TValue` is primitive, read `Arg[0]`.
  * [ ] **Value Extraction (Object)**: If `TValue` is complex, recursively call `DeserializeNode<TValue>`.

* **3.3. Implement `[KdlKeyedNodes]`**
  * [ ] Find all child nodes matching the attribute's `NodeName`.
  * [ ] Loop:
        1. Deserialize child node into `TObject`.
        2. Use Reflection to read `KeyProperty` from `TObject`.
        3. Add `(Key, TObject)` to the dictionary.

---

## Phase 4: Collection & Instantiation

*Handle the "plumbing" of creating objects and lists.*

* **4.1. Factory Logic**
  * [ ] Ensure every target type has a parameterless constructor.
  * [ ] For collections: Handle `List<T>`, `T[]` (needs buffering), and `Dictionary<K,V>`.
  * [ ] Handle **Read-Only Properties**: If a collection property is `get` only but not null, `Clear()` it and reuse the instance rather than trying to set it.

* **4.2. Nullability Safety**
  * [ ] Check for `KdlNull` tokens.
  * [ ] Throw `KdlInvalidCastException` if trying to assign `#null` to `int` or `bool`.

---

## Phase 5: Verification

*Prove it works.*

* **5.1. Test The "Theme/Layout" Scenario**
  * [ ] Create the complex nested Dictionary structure from our previous discussion.
  * [ ] Verify deeply nested recursion works.
* **5.2. Test Failure Modes**
  * [ ] Test "Duplicate Attributes" -> Expect Startup Crash.
  * [ ] Test "Missing Argument Index" -> Expect Startup Crash.
  * [ ] Test "Duplicate Key in Dictionary" -> Expect Deserialization Crash.

## What is deferred (Post-v1)

* Type Annotations logic (`(uuid)`, `(date-time)`).
* Serialization (Writing C# -> KDL).
* Polymorphism (Selecting different derived classes based on KDL annotations).
