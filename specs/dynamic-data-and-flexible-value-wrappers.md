<!-- filepath: specs/dynamic-data-and-flexible-value-wrappers.md -->
# Dynamic Data and Flexible Value Wrappers Specification for pawKitLib

## 1. Overview

This document specifies the requirements and design for **Dynamic Data and Flexible Value Wrappers** in `pawKitLib`. The goal is to provide robust, type-safe, and ergonomic wrappers for handling dynamic, loosely-typed, or schema-less data—especially where values may be `null`, a single value, or a collection of values. This is essential for parsing configuration, API responses (including AI/ML APIs), and any context where data types are not strictly enforced.

All details are based on the design conversation log and the `topics-and-details.md` topic list. Contradictions or ambiguities are resolved in favor of explicitness, minimalism, and testability.

---

## 2. Design Philosophy

- **Type Safety**: Avoid ambiguous or error-prone use of `object` and manual type checks. Prefer discriminated union patterns and explicit wrappers.
- **Ergonomics**: Provide simple, discoverable APIs for common conversions and access patterns (e.g., `.AsStringsOrNull()`, `.AsDoubleOrDefault()`).
- **Null Safety**: All wrappers must handle `null` gracefully and consistently.
- **Immutability**: Value wrappers should be immutable after construction.
- **Extensibility**: Support additional types (e.g., int, double, custom models) as needed.
- **Minimalism**: Avoid AI-generated bloat (e.g., IsSingle/IsMultiple with separate properties). Use a single, well-defined value at a time.

---

## 3. Core Features

### 3.1 Discriminated Union Pattern

- **Value Types Supported**:
  - `null`
  - Single value (e.g., `string`, `int`, `double`, custom model)
  - Array of values (e.g., `string[]`, `int[]`, custom model[])
- **No ambiguous state**: Only one of the above is set at a time.
- **Pattern matching**: Support for C# pattern matching and/or `Match` methods.
- **Implicit conversions**: Allow implicit conversion from supported types to the wrapper.

### 3.2 Wrapper Classes

- **StringValue**: Handles `null`, `string`, or `string[]`.
- **FlexibleValue<T>**: Generic version for other types (e.g., `int`, `double`, custom models).
- **FlexibleEmail**: Example for complex API fields (e.g., `null`, `string`, `EmailModel`, `string[]`, `EmailModel[]`).

### 3.3 Accessor Methods

- `.IsNull`, `.IsSingle`, `.IsArray` (or similar)
- `.GetSingle()`, `.GetArray()`, `.AsEnumerable()`
- `.AsStringsOrNull()`, `.AsDoublesOrNull()`, etc.
- `.AsModelOrNull<T>()` for deserialization to custom types
- `.Match<TResult>(onNull, onSingle, onArray)` for functional access

### 3.4 Type Conversion Helpers

- Centralized conversion logic (e.g., `TypeConversion.ToInt`, `ToDouble`, etc.)
- Sugar methods for direct access (e.g., `.AsIntOrNull()`, `.AsDoubleOrDefault()`).
- Optional: extension methods for additional conversion styles.

### 3.5 JSON Serialization/Deserialization

- Wrappers must be serializable/deserializable with `System.Text.Json`.
- Support for custom converters if needed (e.g., to handle single vs. array values).

---

## 4. Example API Usage

```csharp
// StringValue usage
StringValue val1 = null;
StringValue val2 = "foo";
StringValue val3 = new[] { "a", "b" };

if (val2.IsSingle) Console.WriteLine(val2.GetSingle());
foreach (var s in val3.AsEnumerable()) Console.WriteLine(s);

// FlexibleValue<int>
FlexibleValue<int> intVal = 42;
FlexibleValue<int> intArr = new[] { 1, 2, 3 };

// FlexibleEmail for API responses
FlexibleEmail email = ...;
var addresses = email.AsStringsOrNull();
var models = email.AsEmailsOrNull();
```

---

## 5. Rationale and Best Practices

- **Why discriminated union?**
  - Prevents ambiguous or invalid states (e.g., both single and multiple values set).
  - Enables type-safe, pattern-matchable access.
- **Why not AI-generated IsSingle/IsMultiple?**
  - Avoids bloat and potential data integrity issues.
- **Why centralized conversion?**
  - Ensures consistent, testable, and maintainable type conversions.
- **Why JSON support?**
  - Enables seamless integration with config files and API responses.

---

## 6. File and Namespace Structure

- Namespace: `PawKitLib.Dynamic`
- One class/enum/struct per file; file name matches type name
- All dynamic data wrappers are in the `Dynamic` subfolder and namespace
- Optional: custom JSON converters in a `Dynamic.Json` sub-namespace

---

## 7. References

- All requirements and design decisions are based on the design conversation log and the `topics-and-details.md` topic list
- Contradictions or ambiguities are resolved in favor of explicitness, minimalism, and testability

---

**End of Dynamic Data and Flexible Value Wrappers Specification for pawKitLib**
