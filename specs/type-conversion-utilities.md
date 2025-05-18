<!-- filepath: c:\Repositories\pawKit\specs\type-conversion-utilities.md -->
# Type Conversion Utilities Specification for pawKitLib

## 1. Overview

This document specifies the requirements and design for the **Type Conversion Utilities** in `pawKitLib`. The goal is to provide a centralized, robust, and ergonomic set of helpers for converting between strings and common .NET types, supporting both direct and flexible access patterns. All details are based on the design conversation log and the `topics-and-details.md` topic list. Contradictions or ambiguities are resolved in favor of explicitness, minimalism, and testability.

## 2. Design Philosophy

- **Centralization**: All type conversion logic is implemented in a single utility class (e.g., `TypeConversion`), ensuring consistency and maintainability.
- **Ergonomics**: Provide both direct sugar methods (e.g., `GetIntOrNull`) and extension methods (e.g., `.AsIntOrNull()`) for flexible usage.
- **Minimalism**: Only essential conversions are included; no unnecessary complexity or dependencies.
- **Testability**: All conversion logic is easily testable and mockable.
- **Extensibility**: New types and conversion patterns can be added without breaking existing code.

## 3. Core Features

### 3.1. Supported Conversions

- **String to Primitive**: int, double, bool, DateTime, enums
- **String to Array**: string[], int[], double[], etc. (comma/semicolon/whitespace-separated)
- **String to Model**: JSON deserialization to a model class
- **Null Handling**: All conversions handle nulls gracefully
- **Pluralization**: Methods like `AsStringsOrDefault` always return arrays

### 3.2. API Patterns

#### 3.2.1. Sugar Methods (Direct Access)

- `GetIntOrNull(key)`
- `GetIntOrDefault(key, defaultValue)`
- `GetDoubleOrNull(key)`
- `GetDoubleOrDefault(key, defaultValue)`
- `GetBoolOrNull(key)`
- `GetBoolOrDefault(key, defaultValue)`
- `GetDateTimeOrNull(key)`
- `GetDateTimeOrDefault(key, defaultValue)`
- `GetEnumOrNull<TEnum>(key)`
- `GetEnumOrDefault<TEnum>(key, defaultValue)`
- `GetStringsOrDefault(key, params string[] defaultValue)`

#### 3.2.2. Extension Methods (Chained Access)

- `.AsIntOrNull()`
- `.AsIntOrDefault(defaultValue)`
- `.AsDoubleOrNull()`
- `.AsDoubleOrDefault(defaultValue)`
- `.AsBoolOrNull()`
- `.AsBoolOrDefault(defaultValue)`
- `.AsDateTimeOrNull()`
- `.AsDateTimeOrDefault(defaultValue)`
- `.AsEnumOrNull<TEnum>()`
- `.AsEnumOrDefault<TEnum>(defaultValue)`
- `.AsStringsOrDefault(params string[] defaultValue)`

#### 3.2.3. Model Deserialization

- `.AsModel<T>()` — Deserializes the value to a model class using JSON

### 3.3. Centralized Utility Class

- All conversion logic is implemented in a static class (e.g., `TypeConversion`)
- Extension methods are provided in a separate static class (e.g., `TypeConversionExtensions`)
- All conversions are null-safe and culture-invariant by default
- Optional: support for custom format providers (e.g., for DateTime)

### 3.4. AI-Generated Sugar Methods (Optional)

- For large or dynamic key sets, AI/codegen can generate overloads for common patterns (e.g., `GetIntOrNull("section", "subsection", "key")`)
- These can be wrapped in `#region` or partial classes to avoid clutter

## 4. Implementation Notes

- **No global state**: All conversions are stateless and thread-safe
- **No dependency on external libraries**: Only use .NET BCL and System.Text.Json for model deserialization
- **Culture-invariant parsing**: All numeric and date conversions use `CultureInfo.InvariantCulture`
- **Error handling**: Failed conversions return null or the provided default value; no exceptions are thrown for conversion failures
- **Plural methods**: Always return arrays, never null
- **Extensible**: New types (e.g., Guid, custom structs) can be added as needed

## 5. Example Usage

```csharp
// Direct sugar method
int? port = config.GetIntOrNull("server", "port");

// Extension method
int? port = config.Get("server", "port").AsIntOrNull();

// With default
double timeout = config.Get("network", "timeout").AsDoubleOrDefault(30.0);

// String array
string[] fonts = config.Get("ui", "fonts").AsStringsOrDefault("Arial", "Segoe UI");

// Enum
LogLevel level = config.Get("logging", "level").AsEnumOrDefault(LogLevel.Info);

// Model
var user = config.Get("user", "profile").AsModel<UserProfile>();
```

## 6. Rationale and Best Practices

- **Why centralize?**
  - Ensures all conversions are consistent and easy to update
  - Reduces code duplication and risk of subtle bugs
- **Why both sugar and extension methods?**
  - Direct methods are concise and ergonomic for common cases
  - Extension methods allow for composability and advanced usage
- **Why pluralization?**
  - Always returning arrays avoids null checks and surprises
- **Why AI/codegen for overloads?**
  - Reduces manual maintenance for large/dynamic key sets
  - Keeps the main API surface clean

## 7. File and Namespace Structure

- Namespace: `PawKitLib.Config` (or `PawKitLib.Conversion` if separated)
- One class/enum/struct per file; file name matches type name
- All type conversion-related types are in the `Config` (or `Conversion`) subfolder and namespace

## 8. References

- All requirements and design decisions are based on the design conversation log and the `topics-and-details.md` topic list
- Contradictions or ambiguities are resolved in favor of explicitness, minimalism, and testability

---

**End of Type Conversion Utilities Specification for pawKitLib**
