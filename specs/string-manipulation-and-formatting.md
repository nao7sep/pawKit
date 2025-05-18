# String Manipulation and Formatting Specification for pawKitLib

## 1. Overview

This document specifies the requirements and design for **String Manipulation and Formatting** utilities in `pawKitLib`. The goal is to provide robust, ergonomic, and testable APIs for common and advanced string processing tasks, with a focus on multi-line, user-input, and configuration scenarios. The design is based on the detailed conversation log and the `topics-and-details.md` topic list. Any ambiguities are resolved in favor of explicitness, minimalism, and testability.

---

## 2. Design Philosophy

- **Centralization**: All string manipulation logic is consolidated in a small set of utility and wrapper classes.
- **Minimalism**: Only essential, high-value transformations are included; no over-engineering.
- **Predictability**: All transformations are explicit, documented, and testable.
- **Extensibility**: The model allows for future expansion (e.g., new rules, formatting options) without breaking changes.
- **Immutability**: Where possible, string wrappers are immutable after construction.

---

## 3. Core Utility: `StringLines`

### 3.1 Goals
- Provide static methods for common line- and content-aware string manipulations.
- Support normalization, trimming, whitespace collapsing, and line-based operations.
- Serve as the foundation for higher-level wrappers and extensions.

### 3.2 Methods
- `NormalizeNewlines(string input, string newline = "\n")`: Converts all line endings to the specified style.
- `SplitLines(string input)`: Splits input into lines, handling all newline conventions.
- `TrimLineEnds(string input)`: Trims trailing whitespace from each line.
- `TrimLineStarts(string input)`: Trims leading whitespace from each line.
- `TrimLines(string input)`: Trims both ends of each line.
- `RemoveEmptyLines(string input, bool collapse = false)`: Removes empty lines; if `collapse` is true, collapses multiple empty lines into one.
- `NormalizeIndentation(string input, int spacesPerTab = 4)`: Converts tabs to spaces.
- `CollapseWhitespace(string input)`: Collapses multiple spaces/tabs into a single space.
- `TrimAndNormalize(string input)`: Applies normalization, trimming, and empty line removal in a standard order.

### 3.3 Usage Example
```csharp
var cleaned = StringLines.TrimAndNormalize(messyText);
var withoutEmpty = StringLines.RemoveEmptyLines(messyText, collapse: true);
var normalized = StringLines.NormalizeNewlines(sourceText, newline: "\n");
```

---

## 4. Fluent and Rule-Based Wrappers

### 4.1 `StringFormatter`
- A class that wraps a string and allows chaining of transformation options (e.g., normalize newlines, trim lines, remove/collapse empty lines, set spaces per tab).
- All options are settable via fluent methods.
- The final, processed string is returned by `ToString()`.
- Designed for scenarios where the transformation pipeline is configured at runtime.

#### Example
```csharp
var formatted = new StringFormatter(input)
                    .NormalizeNewlines()
                    .TrimLineEnds()
                    .CollapseEmptyLines()
                    .SpacesPerTab(2)
                    .ToString();
```

### 4.2 `EachLineFormatter`
- A class for per-line rule application.
- Allows chaining of line-based rules (e.g., trim end, remove comments).
- `Apply(string input)` applies all rules to each line and returns the result.

#### Example
```csharp
var cleaned = new EachLineFormatter()
    .TrimEnd()
    .RemoveComment('#')
    .Apply(rawText);
```

---

## 5. Finalizer-Style Input Wrapper: `MultilineInputString`

### 5.1 Purpose
- Encapsulates user or config multi-line input.
- Applies a fixed, built-in set of normalization and cleaning rules (not user-configurable).
- Exposes only the finalized, cleaned string via `ToString()`.
- Provides properties like `IsEmpty` (true if, after cleaning, the input is empty).
- Designed for scenarios where input must be interpreted consistently and safely (e.g., user comments, config fields).

### 5.2 Rules (applied in order)
- Normalize all newlines to `\n`.
- Trim trailing whitespace from each line.
- Remove leading/trailing empty lines.
- Collapse multiple consecutive empty lines into a single empty line.
- Optionally, collapse all whitespace (if required by the use case).

### 5.3 Example
```csharp
var body = new MultilineInputString(rawInput);
var clean = body.ToString();     // Normalized, cleaned string
var isEmpty = body.IsEmpty;      // True if the input is considered empty (after cleaning)
```

---

## 6. Extension Methods

- Provide extension methods for common transformations to allow fluent usage on string instances.
- Example: `input.NormalizeNewlines().TrimLines().CollapseWhitespace();`

---

## 7. File and Namespace Structure

- Namespace: `PawKitLib.Text`
- One class/enum/struct per file, file name matches type name
- All string manipulation classes in the `Text` subfolder and namespace

---

## 8. Usage Guidelines

- **Always use `StringLines` or wrapper classes for string normalization and cleaning.**
- **Do not duplicate string manipulation logic in application code.**
- **Use `MultilineInputString` for all user/config multi-line input fields.**
- **Prefer extension methods for one-off transformations.**

---

## 9. References

- All requirements and design decisions are based on the design conversation log and the `topics-and-details.md` topic list.
- Contradictions or ambiguities are resolved in favor of explicitness, minimalism, and testability.

---

**End of String Manipulation and Formatting Specification for pawKitLib**
