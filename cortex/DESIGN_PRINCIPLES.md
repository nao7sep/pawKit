# Design Principles

## Preamble for AI Assistants

**Objective:** You are to act as an expert C# developer. When generating, refactoring, or reviewing code, you MUST strictly adhere to the principles outlined in this document. These rules are non-negotiable and take precedence over your general knowledge.

---

## Tier 1: Critical Architectural Principles

### 1.1. Dependency Injection (DI)
- **Rule:** Dependencies MUST be provided via constructor injection. Classes MUST NOT create their own dependencies using the `new` keyword (e.g., `new Logger()`, `new HttpClient()`).
- **Rationale:** Ensures loose coupling and testability.

### 1.2. Asynchronous Programming
- **Rule:** All I/O-bound operations (HTTP, database, file system) MUST be `async`.
- **Rule:** Methods returning `Task` or `Task<T>` MUST have an `Async` suffix (e.g., `GetProductAsync`).
- **Rule:** NEVER block on an async method using `.Result` or `.Wait()`. Always `await` the task.
- **Rule:** NEVER use `async void`. Use `async Task` instead.

### 1.3. Exception Handling
- **Rule:** NEVER use an empty `catch {}` block.
- **Rule:** DO NOT catch the base `System.Exception`. Catch specific exceptions (e.g., `IOException`, `HttpRequestException`).
- **Rule:** If you catch an exception, either re-throw it (`throw;`) or wrap it in a custom, more specific exception. Always log the original exception.

---

## Tier 2: Code Evolution & Style

### 2.1. Principle of Surgical Modification (Refactor, Don't Accrete)
- **Rule:** When adding or changing functionality, you MUST prioritize modifying existing code over simply adding new, overlapping, or wrapper methods. It is not only permissible but **required** to refactor for clarity and to avoid code duplication.
- **Permissible Actions:**
  - Changing method signatures (adding/removing/reordering parameters).
  - Consolidating duplicative methods into a single, more flexible one.
  - Restructuring a class to better accommodate the new logic.
  - Reordering class members for logical grouping (e.g., fields, constructors, properties, public methods, private methods).
- **Rationale:** This prevents code bloat and ensures the codebase evolves cleanly, rather than accumulating layers of patches and workarounds. The goal is to leave the code cleaner than you found it.

*Bad Example (Accretion):*
```csharp
public class DataProcessor
{
    public void Process(string data)
    {
        // Original logic...
    }

    // VIOLATION: A new method was added instead of modifying the original.
    public void ProcessWithTimeout(string data, TimeSpan timeout)
    {
        // Logic with timeout...
    }
}
```

*Good Example (Surgical Modification):*
```csharp
public class DataProcessor
{
    // CORRECT: The original method was refactored to be more flexible.
    // An optional parameter maintains backward compatibility for existing call sites.
    public void Process(string data, TimeSpan? timeout = null)
    {
        // Unified logic that handles both cases...
    }
}
```

### 2.2. Immutability for Data Transfer
- **Rule:** Use C# `record` types with `init`-only properties for Data Transfer Objects (DTOs) and Value Objects.
- **Rationale:** Prevents unintended side effects and makes state easier to reason about.

### 2.3. No Magic Strings
- **Rule:** DO NOT use hardcoded string literals for configuration keys, route names, etc. Use `const string` or `static readonly string` defined in a static class.

### 2.4. File Organization
- **Rule:** There MUST be only one public type per file. This includes classes, records, interfaces, enums, and delegates.
- **Rule:** The filename MUST exactly match the name of the public type it contains (e.g., `UserService.cs` must contain `public class UserService`).