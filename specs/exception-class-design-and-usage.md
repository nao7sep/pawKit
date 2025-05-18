<!-- filepath: specs/exception-class-design-and-usage.md -->
# Exception Class Design and Usage Specification for pawKitLib

## 1. Overview

This document specifies the requirements and design for **Exception Class Design and Usage** in `pawKitLib`. The goal is to provide a clear, maintainable, and extensible exception hierarchy and usage guidelines, ensuring robust error handling, clear diagnostics, and a consistent developer experience. All details are based on the design conversation log and the `topics-and-details.md` topic list. Contradictions or ambiguities are resolved in favor of explicitness, minimalism, and future-proofing.

---

## 2. Design Philosophy

- **Clarity and Intent**: Exception types should clearly communicate the nature and source of errors.
- **Minimalism**: Only introduce custom exceptions when they add value beyond built-in .NET exceptions.
- **Extensibility**: The hierarchy should allow for future domain-specific exceptions without breaking changes.
- **Consistency**: Naming, structure, and usage patterns are uniform across the library.
- **Separation of Concerns**: Exception classes are distinct from exception management/serialization (see separate spec).

---

## 3. When to Use Built-in vs. Custom Exceptions

- **Use built-in .NET exceptions** (e.g., `ArgumentNullException`, `InvalidOperationException`, `FormatException`) for common error conditions that are well-understood and unambiguous.
- **Introduce custom exceptions** only when:
  - The error is domain-specific (e.g., configuration parsing, validation, file system policy violation).
  - Additional context or structured data is needed.
  - You want to enable consumers to catch specific errors from `pawKitLib` without ambiguity.
- **Never** create custom exceptions for generic errors already covered by .NET (e.g., do not create `NullArgumentException`).

---

## 4. Exception Hierarchy and Naming Conventions

- All custom exceptions inherit from a single base class: `PawKitLibException` (inherits from `System.Exception`).
- Domain-specific exceptions inherit from `PawKitLibException`.
- Naming: `[Domain][Operation]Exception` (e.g., `ConfigParseException`, `PathPolicyViolationException`).
- All exception classes are suffixed with `Exception`.
- Each exception is defined in its own file, named after the class.
- Namespace: `PawKitLib.Exceptions`.

---

## 5. Base Exception Class

```csharp
namespace PawKitLib.Exceptions
{
    public class PawKitLibException : Exception
    {
        public PawKitLibException() { }
        public PawKitLibException(string message) : base(message) { }
        public PawKitLibException(string message, Exception inner) : base(message, inner) { }
    }
}
```

- All custom exceptions must inherit from this base class.
- Allows for broad catch blocks (`catch (PawKitLibException ex)`) when needed.

---

## 6. Domain-Specific Exception Examples

### 6.1 Configuration Parsing

```csharp
namespace PawKitLib.Exceptions
{
    public class ConfigParseException : PawKitLibException
    {
        public string ConfigPath { get; }
        public ConfigParseException(string path, string message) : base($"Config parse error at '{path}': {message}")
        {
            ConfigPath = path;
        }
        public ConfigParseException(string path, string message, Exception inner) : base($"Config parse error at '{path}': {message}", inner)
        {
            ConfigPath = path;
        }
    }
}
```

### 6.2 Path Policy Violation

```csharp
namespace PawKitLib.Exceptions
{
    public class PathPolicyViolationException : PawKitLibException
    {
        public string Path { get; }
        public PathPolicyViolationException(string path, string message) : base($"Path policy violation for '{path}': {message}")
        {
            Path = path;
        }
        public PathPolicyViolationException(string path, string message, Exception inner) : base($"Path policy violation for '{path}': {message}", inner)
        {
            Path = path;
        }
    }
}
```

- Additional domain-specific exceptions (e.g., `ValidationException`, `IdGenerationException`) follow the same pattern.

---

## 7. Wrapping and Translating Built-in Exceptions

- When catching a built-in exception that represents a domain-specific failure, wrap it in a custom exception to provide context:

```csharp
try
{
    // ...
}
catch (FormatException ex)
{
    throw new ConfigParseException(path, "Invalid format", ex);
}
```

- This preserves the original stack trace and error details while providing a clear, domain-specific error type.

---

## 8. Usage Guidelines

- **Throw early, throw specific**: Use the most specific exception type available.
- **Never swallow exceptions**: Always log or propagate.
- **Document thrown exceptions** in XML comments.
- **Avoid using exceptions for control flow**: Use them only for truly exceptional conditions.
- **Preserve inner exceptions** when wrapping.

---

## 9. File and Namespace Structure

- Namespace: `PawKitLib.Exceptions`
- One class per file, file name matches type name
- All exception classes in the `Exceptions` subfolder and namespace

---

## 10. References

- All requirements and design decisions are based on the design conversation log and the `topics-and-details.md` topic list
- Contradictions or ambiguities are resolved in favor of explicitness, minimalism, and testability

---

**End of Exception Class Design and Usage Specification for pawKitLib**
