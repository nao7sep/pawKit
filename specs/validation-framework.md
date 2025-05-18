# Validation Framework Specification for pawKitLib

## 1. Overview

This document specifies the requirements and design for the **Validation Framework** in `pawKitLib`. The goal is to provide a fluent, testable, and extensible validation engine for .NET applications, supporting both simple and advanced validation scenarios. The design is based on the detailed conversation log and the `topics-and-details.md` topic list. Any ambiguities are resolved in favor of explicitness, minimalism, and testability.

---

## 2. Design Philosophy

- **Fluent API**: Validation rules are defined using a fluent, chainable syntax for readability and composability.
- **Testability**: All validation logic is easily unit-testable and deterministic.
- **Minimalism**: Only essential features are included; no unnecessary complexity or dependencies.
- **Extensibility**: New rules and validation strategies can be added without breaking changes.
- **Separation of Concerns**: Validation logic is decoupled from model classes and business logic.
- **No Dependency Bloat**: No dependency on external validation libraries (e.g., FluentValidation) in the core.
- **Optional Attributes**: Attribute-based validation is supported optionally, but not tightly coupled to the core engine.

---

## 3. Core Interfaces and Classes

### 3.1. `Validator<T>`
- **Purpose**: Entry point for defining and running validation rules on objects of type `T`.
- **API**:
  - `Validator.For(T instance)` — creates a validator for the given instance.
  - `.Rule(Func<T, bool> predicate, string errorMessage)` — adds a custom rule.
  - Built-in rule methods (see below).
  - `.Run()` — executes all rules and returns a `ValidationResult`.
- **Usage**: All validation is performed via `Validator<T>` instances.

### 3.2. `ValidationResult`
- **Purpose**: Represents the outcome of a validation run.
- **API**:
  - `bool IsValid` — true if no errors.
  - `IReadOnlyList<string> Errors` — list of error messages.
  - `static ValidationResult Success` — singleton for valid results.
- **Usage**: Returned by all validation operations.

### 3.3. Built-in Rules
- **NotNullOrEmpty**: Ensures a string or collection is not null or empty.
- **Email**: Validates that a string is a valid email address.
- **RegexMatch**: Validates that a string matches a given regex pattern.
- **Range**: Validates that a value is within a specified range.
- **Custom**: Any user-defined predicate.

#### Example
```csharp
var result = Validator.For(user)
    .NotNullOrEmpty(u => u.Email, "Email is required.")
    .Email(u => u.Email, "Invalid email format.")
    .Run();
if (!result.IsValid) Console.WriteLine(string.Join("; ", result.Errors));
```

---

## 4. Advanced Features

### 4.1. Custom Rule Registration
- Users can register custom rules globally or per-validator.
- Rules can be composed and reused.

### 4.2. Attribute-Based Validation (Optional)
- **Purpose**: Support for model-based validation using custom attributes (e.g., `[NotNullOrEmpty]`).
- **Design**: Attribute validation is implemented as an optional layer, not tightly coupled to the core engine.
- **Usage**: `Validator.For(model).FromAttributes().Run();`

### 4.3. Validation Context
- **Purpose**: Passes additional context (e.g., culture, external data) to rules if needed.
- **Design**: Context is optional and can be attached to a validator instance.

---

## 5. API Patterns

### 5.1. Fluent Rule Definition
- Rules are defined in a chainable, readable manner.
- Each rule adds to the validator's internal rule list.

### 5.2. ValidationResult Handling
- All validation returns a `ValidationResult`.
- No exceptions are thrown for validation failures (unless explicitly requested).
- Errors are collected and reported together.

---

## 6. Example Usage

```csharp
// Fluent validation
var result = Validator.For(user)
    .NotNullOrEmpty(u => u.Name, "Name required.")
    .Range(u => u.Age, 0, 120, "Age must be between 0 and 120.")
    .Run();

if (!result.IsValid)
    foreach (var error in result.Errors)
        Console.WriteLine(error);

// Attribute-based validation (optional)
var result2 = Validator.For(user).FromAttributes().Run();
```

---

## 7. File and Namespace Structure

- Namespace: `PawKitLib.Validation`
- One class/enum/struct per file, file name matches type name
- All validation-related classes in the `Validation` subfolder and namespace

---

## 8. Usage Guidelines

- **Always use `Validator<T>` for validation logic.**
- **Prefer fluent rule definitions for clarity and testability.**
- **Use attribute-based validation only when model-driven validation is required.**
- **Do not throw exceptions for validation failures; use `ValidationResult`.**
- **Write custom rules as needed and register them for reuse.**

---

## 9. References

- All requirements and design decisions are based on the design conversation log and the `topics-and-details.md` topic list.
- Contradictions or ambiguities are resolved in favor of explicitness, minimalism, and testability.

---

**End of Validation Framework Specification for pawKitLib**
